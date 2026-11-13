using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class ReturnRouteResourceFromEntityAssembler
{
    public static ReturnRouteResource ToResourceFromEntity(ReturnRoute route,
        IReadOnlyDictionary<VehicleId, Guid> vehiclePublicIds)
    {
        return new ReturnRouteResource(
            route.PublicId,
            route.CarrierProfileId,
            vehiclePublicIds.TryGetValue(route.VehicleId, out var vehiclePublicId) ? vehiclePublicId : null,
            route.DriverId?.Value,
            route.Origin.Address,
            GeoLocationResourceAssembler.ToResourceFromValueObject(route.Origin.Location),
            route.Destination.Address,
            GeoLocationResourceAssembler.ToResourceFromValueObject(route.Destination.Location),
            route.DepartureAt,
            route.MaxShipments,
            route.Status.ToSnakeCaseValue(),
            route.CreatedAt);
    }
}
