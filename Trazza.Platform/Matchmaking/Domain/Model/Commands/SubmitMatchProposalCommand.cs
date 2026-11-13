using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

public record SubmitMatchProposalCommand(
    UserId UserId,
    ShipmentRequestId ShipmentRequestId,
    ReturnRouteId ReturnRouteId,
    Money OfferedRate);
