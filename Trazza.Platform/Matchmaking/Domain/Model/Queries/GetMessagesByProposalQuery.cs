using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

public record GetMessagesByProposalQuery(UserId UserId, MatchProposalId MatchProposalId);
