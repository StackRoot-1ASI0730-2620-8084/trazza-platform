using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

/// <summary>
///     Marks as read the messages of a proposal that were sent by the other participant.
/// </summary>
public record MarkMessagesAsReadCommand(UserId UserId, MatchProposalId MatchProposalId);
