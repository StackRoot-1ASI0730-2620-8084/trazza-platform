using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

namespace Trazza.Platform.Matchmaking.Domain.Services;

/// <summary>
///     Rules that decide whether a return route is a good match for a shipment request.
///     The capacity of the vehicle is checked apart, when a proposal is accepted.
/// </summary>
public interface IRouteMatchingService
{
    /// <summary>
    ///     A route is compatible when it is published, the request is open, the route starts near the pickup,
    ///     ends near the delivery, and leaves inside the pickup window.
    /// </summary>
    bool IsCompatible(ReturnRoute route, ShipmentRequest request, double maxDistanceKm);

    /// <summary>
    ///     Sum in kilometers of the distance from the route origin to the pickup and from the route
    ///     destination to the delivery. The lower, the better the match.
    /// </summary>
    double DistanceScoreKm(ReturnRoute route, ShipmentRequest request);
}
