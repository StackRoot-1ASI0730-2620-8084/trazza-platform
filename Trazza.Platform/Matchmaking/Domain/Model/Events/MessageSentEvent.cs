using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Matchmaking.Domain.Model.Events;

/// <summary>
///     Raised when a user sends a chat message inside a proposal.
/// </summary>
public record MessageSentEvent(MatchProposalId MatchProposalId, UserId SenderUserId) : IEvent;
