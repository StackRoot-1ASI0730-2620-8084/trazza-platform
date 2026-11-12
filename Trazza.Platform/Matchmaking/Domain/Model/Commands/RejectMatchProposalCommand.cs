using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

public record RejectMatchProposalCommand(UserId UserId, MatchProposalId MatchProposalId);
