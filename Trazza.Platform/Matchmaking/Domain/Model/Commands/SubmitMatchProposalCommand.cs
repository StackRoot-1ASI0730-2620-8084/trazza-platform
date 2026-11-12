using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

/// <summary>
///     A carrier offers one of its routes, with a rate, for a shipment request.
/// </summary>
public record SubmitMatchProposalCommand(
    UserId UserId,
    ShipmentRequestId ShipmentRequestId,
    ReturnRouteId ReturnRouteId,
    Money OfferedRate);
