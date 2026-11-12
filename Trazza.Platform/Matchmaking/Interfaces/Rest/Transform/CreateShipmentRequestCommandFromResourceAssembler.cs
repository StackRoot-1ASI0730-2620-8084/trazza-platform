using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class CreateShipmentRequestCommandFromResourceAssembler
{
    public static CreateShipmentRequestCommand ToCommandFromResource(UserId userId,
        CreateShipmentRequestResource resource)
    {
        return new CreateShipmentRequestCommand(
            userId,
            resource.Description,
            new Cargo(resource.WeightKg, resource.VolumeM3),
            new AddressPoint((resource.PickupAddress ?? string.Empty).Trim(),
                GeoLocationResourceAssembler.ToValueObjectFromResource(resource.PickupLocation)),
            new AddressPoint((resource.DeliveryAddress ?? string.Empty).Trim(),
                GeoLocationResourceAssembler.ToValueObjectFromResource(resource.DeliveryLocation)),
            new TimeWindow(resource.PickupFrom, resource.PickupUntil));
    }
}
