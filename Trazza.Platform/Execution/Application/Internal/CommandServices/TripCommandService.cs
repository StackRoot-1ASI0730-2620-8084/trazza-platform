using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Model.Commands;
using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Matchmaking.Interfaces.Acl;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Execution.Application.Internal.CommandServices;

/// <summary>
///     Trip life cycle. Delivering opens the freight settlement, confirming awards points and deletes the chat, and a
///     merchant who cancels after the route left owes lost profit (30% of the agreed rate) to the carrier.
/// </summary>
public class TripCommandService(
    TripAccessService tripAccessService,
    IMatchmakingContextFacade matchmakingContextFacade,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ITripCommandService
{
    public const decimal LostProfitRate = 0.30m;

    public async Task<Result<TripView, CommandError>> Handle(AssignDriverCommand command, CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        if (!access.IsCarrier) return CommandError.Forbidden("only-carrier");

        var driver = await iamContextFacade.FindDriverAsync(command.DriverId, ct);
        if (driver is null || driver.CarrierProfileId != access.Context.CarrierProfileId)
            return CommandError.NotFound("driver-not-found");
        if (!driver.IsActive) return CommandError.Conflict("driver-inactive");

        return await SaveAsync(access, access.Trip.AssignDriver(driver.Id), ct);
    }

    public async Task<Result<TripView, CommandError>> Handle(PickUpTripCommand command, CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        if (!access.IsCarrier) return CommandError.Forbidden("only-carrier");

        var result = access.Trip.PickUp(command.UserId, timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsSuccess) await matchmakingContextFacade.MarkPickedUpAsync(access.Trip.MatchProposalId, ct);
        return await SaveAsync(access, result, ct);
    }

    public Task<Result<TripView, CommandError>> Handle(DeliverTripCommand command, CancellationToken ct = default) =>
        unitOfWork.ExecuteInTransactionAsync(() => DeliverAsync(command, ct), result => result.IsSuccess, ct);

    private async Task<Result<TripView, CommandError>> DeliverAsync(DeliverTripCommand command, CancellationToken ct)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        if (!access.IsCarrier) return CommandError.Forbidden("only-carrier");

        var result = access.Trip.Deliver(command.UserId, timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsSuccess) await matchmakingContextFacade.MarkDeliveredAsync(access.Trip.MatchProposalId, ct);

        return await SaveAsync(access, result, ct);
    }

    public Task<Result<TripView, CommandError>> Handle(ConfirmTripCommand command, CancellationToken ct = default) =>
        unitOfWork.ExecuteInTransactionAsync(() => ConfirmAsync(command, ct), result => result.IsSuccess, ct);

    private async Task<Result<TripView, CommandError>> ConfirmAsync(ConfirmTripCommand command, CancellationToken ct)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        if (!access.IsMerchant) return CommandError.Forbidden("only-merchant");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var result = access.Trip.Confirm(command.UserId, now);
        if (result.IsSuccess) await matchmakingContextFacade.PurgeChatAsync(access.Trip.MatchProposalId, ct);

        return await SaveAsync(access, result, ct);
    }

    public Task<Result<TripView, CommandError>> Handle(CancelTripCommand command, CancellationToken ct = default) =>
        unitOfWork.ExecuteInTransactionAsync(() => CancelAsync(command, ct), result => result.IsSuccess, ct);

    private async Task<Result<TripView, CommandError>> CancelAsync(CancelTripCommand command, CancellationToken ct)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var result = access.Trip.Cancel(command.UserId, command.Reason, now);
        if (result.IsSuccess)
        {
            await matchmakingContextFacade.ReleaseAsync(access.Trip.MatchProposalId, access.IsMerchant,
                command.Reason, now, ct);
            await matchmakingContextFacade.PurgeChatAsync(access.Trip.MatchProposalId, ct);
        }

        return await SaveAsync(access, result, ct);
    }

    private async Task<Result<TripView, CommandError>> SaveAsync(TripAccess access, Result<Trip, CommandError> result,
        CancellationToken ct)
    {
        if (result.IsFailure) return result.Error!;
        await unitOfWork.CompleteAsync(ct);
        return await tripAccessService.BuildViewAsync(access, ct);
    }
}
