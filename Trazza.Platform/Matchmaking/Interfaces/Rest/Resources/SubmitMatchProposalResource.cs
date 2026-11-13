using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record SubmitMatchProposalResource(Guid ShipmentRequestId, Guid ReturnRouteId, MoneyResource OfferedRate);
