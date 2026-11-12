using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class ReturnRouteResourceFromEntityAssembler
{
    public static ReturnRouteResource ToResourceFromEntity(ReturnRoute route)
    {
        return new ReturnRouteResource(
            route.Id,
            route.CarrierProfileId,
            route.VehicleId,
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
