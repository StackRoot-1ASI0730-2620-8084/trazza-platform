using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

/// <summary>
///     Gets the chat of a proposal. The user must be one of its two participants.
/// </summary>
public record GetMessagesByProposalQuery(UserId UserId, MatchProposalId MatchProposalId);
