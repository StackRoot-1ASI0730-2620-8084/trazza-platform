using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

/// <summary>
///     Published routes that fit a shipment request of the merchant who asks.
/// </summary>
public record GetSuggestedReturnRoutesQuery(UserId UserId, ShipmentRequestId ShipmentRequestId);
