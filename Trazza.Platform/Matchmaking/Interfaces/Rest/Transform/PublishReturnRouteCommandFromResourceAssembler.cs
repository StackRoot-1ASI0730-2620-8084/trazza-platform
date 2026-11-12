using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class PublishReturnRouteCommandFromResourceAssembler
{
    public static PublishReturnRouteCommand ToCommandFromResource(UserId userId, PublishReturnRouteResource resource)
    {
        return new PublishReturnRouteCommand(
            userId,
            new VehicleId(resource.VehicleId),
            resource.DriverId is { } driverId ? new DriverId(driverId) : null,
            new AddressPoint((resource.OriginAddress ?? string.Empty).Trim(),
                GeoLocationResourceAssembler.ToValueObjectFromResource(resource.OriginLocation)),
            new AddressPoint((resource.DestinationAddress ?? string.Empty).Trim(),
                GeoLocationResourceAssembler.ToValueObjectFromResource(resource.DestinationLocation)),
            resource.DepartureAt,
            resource.MaxShipments);
    }
}
