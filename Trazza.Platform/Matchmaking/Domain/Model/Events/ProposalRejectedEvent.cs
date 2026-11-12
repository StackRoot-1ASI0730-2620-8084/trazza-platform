using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Matchmaking.Domain.Model.Events;

/// <summary>
///     Raised when a merchant rejects a proposal.
/// </summary>
public record ProposalRejectedEvent(MatchProposalId MatchProposalId, ShipmentRequestId ShipmentRequestId) : IEvent;
