using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

public record CancelMatchProposalCommand(UserId UserId, MatchProposalId MatchProposalId);
