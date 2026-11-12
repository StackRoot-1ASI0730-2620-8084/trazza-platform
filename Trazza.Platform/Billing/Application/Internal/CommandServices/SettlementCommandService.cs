using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Billing.Domain.Services;
using Trazza.Platform.Execution.Interfaces.Acl;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Repositories;
 
namespace Trazza.Platform.Billing.Application.Internal.CommandServices;

public class SettlementCommandService(
    IFreightSettlementRepository settlementRepository,
    ICancellationCompensationRepository compensationRepository,
    IExecutionContextFacade executionContextFacade,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ISettlementCommandService
{
    private const int MaxReferenceLength = 100;
 
    public async Task<Result<FreightSettlementView, CommandError>> Handle(MarkFreightPaidCommand command,
        CancellationToken ct = default)
    {
        var trip = await executionContextFacade.FindTripAsync(command.TripId, ct);
        if (trip is null || (trip.MerchantUserId != command.UserId && trip.CarrierUserId != command.UserId))
            return CommandError.NotFound("trip-not-found");
        if (trip.MerchantUserId != command.UserId) return CommandError.Forbidden("only-merchant");
        if (command.ExternalReference?.Trim().Length > MaxReferenceLength)
            return CommandError.Validation("external-reference-invalid");
 
        var settlement = await settlementRepository.FindByIdAsync(trip.Id, ct);
        if (settlement is null) return CommandError.NotFound("settlement-not-found");
 
        settlement.MarkPaid(command.ExternalReference, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.CompleteAsync(ct);
        return new FreightSettlementView(settlement, trip, false);
    }
 
    public async Task<Result<FreightSettlementView, CommandError>> Handle(ConfirmFreightReceivedCommand command,
        CancellationToken ct = default)
    {
        var trip = await executionContextFacade.FindTripAsync(command.TripId, ct);
        if (trip is null || (trip.MerchantUserId != command.UserId && trip.CarrierUserId != command.UserId))
            return CommandError.NotFound("trip-not-found");
        if (trip.CarrierUserId != command.UserId) return CommandError.Forbidden("only-carrier");
 
        var settlement = await settlementRepository.FindByIdAsync(trip.Id, ct);
        if (settlement is null) return CommandError.NotFound("settlement-not-found");
 
        settlement.ConfirmReceived(timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.CompleteAsync(ct);
        return new FreightSettlementView(settlement, trip, true);
    }
 
    public async Task<Result<CompensationView, CommandError>> Handle(UpdateCompensationCommand command,
        CancellationToken ct = default)
    {
        var trip = await executionContextFacade.FindTripAsync(command.TripId, ct);
        if (trip is null || (trip.MerchantUserId != command.UserId && trip.CarrierUserId != command.UserId))
            return CommandError.NotFound("trip-not-found");
 
        var compensation = await compensationRepository.FindByIdAsync(trip.Id, ct);
        if (compensation is null) return CommandError.NotFound("compensation-not-found");
        if (!compensation.IsOpen) return CommandError.Conflict("compensation-settled");
 
        var isCarrier = trip.CarrierUserId == command.UserId;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        switch (command.Action)
        {
            case CompensationAction.ConfirmPaid when isCarrier:
                compensation.ConfirmPaid(now);
                break;
            case CompensationAction.Waive when isCarrier:
                compensation.Waive(now);
                break;
            case CompensationAction.Dispute when !isCarrier:
                compensation.Dispute();
                break;
            default:
                return CommandError.Forbidden(isCarrier ? "only-merchant" : "only-carrier");
        }
 
        await unitOfWork.CompleteAsync(ct);
        return new CompensationView(compensation, trip, isCarrier);
    }
}
