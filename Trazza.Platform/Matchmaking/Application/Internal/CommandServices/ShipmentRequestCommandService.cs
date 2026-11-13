using Trazza.Platform.Matchmaking.Application.CommandServices;
using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Application.Internal.CommandServices;

public class ShipmentRequestCommandService(
    IShipmentRequestRepository shipmentRequestRepository,
    IProfilesService profilesService,
    IUnitOfWork unitOfWork)
    : IShipmentRequestCommandService
{
    public async Task<Result<ShipmentRequest>> Handle(CreateShipmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var description = (command.Description ?? string.Empty).Trim();

        if (description.Length == 0 || description.Length > ShipmentRequest.MaxDescriptionLength)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.InvalidDescription);
        if (!command.Cargo.IsValid)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.InvalidCargo);
        if (!command.Pickup.IsValid || !command.Delivery.IsValid)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.InvalidAddress);
        if (!command.PickupWindow.IsValid || command.PickupWindow.Until <= DateTimeOffset.UtcNow)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.InvalidTimeWindow);

        var merchantProfileId =
            await profilesService.FetchMerchantProfileIdByUserId(command.UserId, cancellationToken);
        if (merchantProfileId is null)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.MerchantProfileNotFound);

        var request = new ShipmentRequest(merchantProfileId.Value, description, command.Cargo, command.Pickup,
            command.Delivery, command.PickupWindow);

        await shipmentRequestRepository.AddAsync(request, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        request.RaiseCreated();
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<ShipmentRequest>.Success(request);
    }

    public async Task<Result<ShipmentRequest>> Handle(CancelShipmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var merchantProfileId =
            await profilesService.FetchMerchantProfileIdByUserId(command.UserId, cancellationToken);
        if (merchantProfileId is null)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.MerchantProfileNotFound);

        var request = await shipmentRequestRepository.FindByIdAsync(command.ShipmentRequestId, cancellationToken);
        if (request is null)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.ShipmentRequestNotFound);
        if (request.MerchantProfileId != merchantProfileId.Value)
            return Result<ShipmentRequest>.Failure(MatchmakingErrors.NotShipmentOwner);

        var reason = command.Reason?.Trim();
        if (reason is { Length: > ShipmentRequest.MaxCancellationReasonLength })
            reason = reason[..ShipmentRequest.MaxCancellationReasonLength];

        var error = request.Cancel(reason, DateTimeOffset.UtcNow);
        if (!error.IsNone)
            return Result<ShipmentRequest>.Failure(error);

        shipmentRequestRepository.Update(request);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<ShipmentRequest>.Success(request);
    }
}
