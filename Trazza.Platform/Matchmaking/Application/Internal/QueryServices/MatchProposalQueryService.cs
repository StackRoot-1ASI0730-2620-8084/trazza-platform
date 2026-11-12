using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Application.Internal.QueryServices;

public class MatchProposalQueryService(IMatchProposalRepository matchProposalRepository) : IMatchProposalQueryService
{
    public async Task<MatchProposal?> Handle(GetMatchProposalByIdQuery query, CancellationToken cancellationToken)
    {
        return await matchProposalRepository.FindByIdAsNoTrackingAsync(query.MatchProposalId, cancellationToken);
    }

    public async Task<IEnumerable<MatchProposal>> Handle(GetProposalsByRouteQuery query,
        CancellationToken cancellationToken)
    {
        return await matchProposalRepository.FindByReturnRouteIdAsNoTrackingAsync(query.ReturnRouteId,
            cancellationToken);
    }

    public async Task<IEnumerable<MatchProposal>> Handle(GetProposalsByShipmentQuery query,
        CancellationToken cancellationToken)
    {
        return await matchProposalRepository.FindByShipmentRequestIdAsNoTrackingAsync(query.ShipmentRequestId,
            cancellationToken);
    }
}
