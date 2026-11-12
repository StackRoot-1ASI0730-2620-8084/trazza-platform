using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

public record SendMessageCommand(UserId UserId, MatchProposalId MatchProposalId, string Content);
