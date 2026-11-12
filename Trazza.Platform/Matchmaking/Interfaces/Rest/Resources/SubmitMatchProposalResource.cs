using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record SubmitMatchProposalResource(ulong ShipmentRequestId, ulong ReturnRouteId, MoneyResource OfferedRate);
