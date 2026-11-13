using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record MatchProposalResource(
    Guid Id,
    Guid ShipmentRequestId,
    Guid ReturnRouteId,
    MoneyResource OfferedRate,
    string Status,
    DateTimeOffset? RespondedAt,
    DateTimeOffset? CreatedAt,
    Guid? TripId);
