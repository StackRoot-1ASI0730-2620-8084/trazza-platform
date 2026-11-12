using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

public record AcceptMatchProposalCommand(UserId UserId, MatchProposalId MatchProposalId);
