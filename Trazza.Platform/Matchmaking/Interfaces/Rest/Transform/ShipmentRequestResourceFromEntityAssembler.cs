using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class ShipmentRequestResourceFromEntityAssembler
{
    public static ShipmentRequestResource ToResourceFromEntity(ShipmentRequest request)
    {
        return new ShipmentRequestResource(
            request.PublicId,
            request.MerchantProfileId,
            request.Description,
            request.Cargo.WeightKg,
            request.Cargo.VolumeM3,
            request.Pickup.Address,
            GeoLocationResourceAssembler.ToResourceFromValueObject(request.Pickup.Location),
            request.Delivery.Address,
            GeoLocationResourceAssembler.ToResourceFromValueObject(request.Delivery.Location),
            request.PickupWindow.From,
            request.PickupWindow.Until,
            request.Status.ToSnakeCaseValue(),
            request.CancelledAt,
            request.CancellationReason,
            request.CreatedAt);
    }
}
