using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

/// <summary>
///     Finds published routes that start near the pickup point and end near the delivery point,
///     and that leave inside the pickup window. Used by a merchant to find carriers.
/// </summary>
public record SearchReturnRoutesQuery(
    GeoLocation Pickup,
    GeoLocation Delivery,
    TimeWindow PickupWindow,
    double MaxDistanceKm = SearchReturnRoutesQuery.DefaultMaxDistanceKm)
{
    public const double DefaultMaxDistanceKm = 25;
}
