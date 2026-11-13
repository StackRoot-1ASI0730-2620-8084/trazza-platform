using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

/// <summary>
///     Open shipment requests that fit a route of the carrier who asks: pickup near the origin, delivery near the
///     destination and a pickup window that includes the departure.
/// </summary>
public record GetSuggestedShipmentRequestsQuery(UserId UserId, ReturnRouteId ReturnRouteId);
