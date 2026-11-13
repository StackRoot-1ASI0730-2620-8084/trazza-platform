using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record MatchProposalResource(
    ulong Id,
    ulong ShipmentRequestId,
    ulong ReturnRouteId,
    MoneyResource OfferedRate,
    string Status,
    DateTimeOffset? RespondedAt,
    DateTimeOffset? CreatedAt,
    Guid? TripId);
