using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Services;

namespace Trazza.Platform.Matchmaking.Infrastructure.Services;

public class RouteMatchingService : IRouteMatchingService
{
    public bool IsCompatible(ReturnRoute route, ShipmentRequest request, double maxDistanceKm)
    {
        return route.IsPublished &&
               request.IsOpen &&
               request.PickupWindow.Contains(route.DepartureAt) &&
               route.Origin.Location.DistanceKmTo(request.Pickup.Location) <= maxDistanceKm &&
               route.Destination.Location.DistanceKmTo(request.Delivery.Location) <= maxDistanceKm;
    }

    public double DistanceScoreKm(ReturnRoute route, ShipmentRequest request)
    {
        return route.Origin.Location.DistanceKmTo(request.Pickup.Location) +
               route.Destination.Location.DistanceKmTo(request.Delivery.Location);
    }
}
