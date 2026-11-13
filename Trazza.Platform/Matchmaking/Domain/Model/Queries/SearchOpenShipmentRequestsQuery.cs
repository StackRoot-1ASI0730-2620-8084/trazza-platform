using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

public record SearchOpenShipmentRequestsQuery(
    GeoLocation Origin,
    GeoLocation Destination,
    DateTimeOffset DepartureAt,
    double MaxDistanceKm = SearchReturnRoutesQuery.DefaultMaxDistanceKm);
