using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

public record SearchReturnRoutesQuery(
    GeoLocation Pickup,
    GeoLocation Delivery,
    TimeWindow PickupWindow,
    double MaxDistanceKm = SearchReturnRoutesQuery.DefaultMaxDistanceKm)
{
    public const double DefaultMaxDistanceKm = 25;
}
