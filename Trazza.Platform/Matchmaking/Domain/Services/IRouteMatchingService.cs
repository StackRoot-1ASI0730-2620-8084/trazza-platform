using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

namespace Trazza.Platform.Matchmaking.Domain.Services;

public interface IRouteMatchingService
{
    bool IsCompatible(ReturnRoute route, ShipmentRequest request, double maxDistanceKm);

    double DistanceScoreKm(ReturnRoute route, ShipmentRequest request);
}
