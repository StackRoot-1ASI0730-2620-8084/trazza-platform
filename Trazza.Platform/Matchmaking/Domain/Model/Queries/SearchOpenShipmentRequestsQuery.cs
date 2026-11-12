using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

/// <summary>
///     Finds open shipment requests whose pickup is near the origin of a route, whose delivery is near
///     its destination, and whose pickup window includes the departure. Used by a carrier to find cargo.
/// </summary>
public record SearchOpenShipmentRequestsQuery(
    GeoLocation Origin,
    GeoLocation Destination,
    DateTimeOffset DepartureAt,
    double MaxDistanceKm = SearchReturnRoutesQuery.DefaultMaxDistanceKm);
