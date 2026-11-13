using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IMatchProposalQueryService
{
    Task<MatchProposal?> Handle(GetMatchProposalByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> Handle(GetProposalsByRouteQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> Handle(GetProposalsByShipmentQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> Handle(GetProposalsByUserQuery query, CancellationToken cancellationToken);
}
