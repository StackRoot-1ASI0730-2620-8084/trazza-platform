using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Application.Internal.QueryServices;

public class MatchProposalQueryService(
    IMatchProposalRepository matchProposalRepository,
    IReturnRouteRepository returnRouteRepository,
    IShipmentRequestRepository shipmentRequestRepository,
    IProfilesService profilesService)
    : IMatchProposalQueryService
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

    public async Task<IEnumerable<MatchProposal>> Handle(GetProposalsByUserQuery query,
        CancellationToken cancellationToken)
    {
        var proposals = new List<MatchProposal>();

        if (await profilesService.FetchCarrierProfileIdByUserId(query.UserId, cancellationToken) is { } carrierId)
        {
            var routeIds = (await returnRouteRepository.FindByCarrierProfileIdAsNoTrackingAsync(carrierId,
                cancellationToken)).Select(route => route.Id).ToList();
            proposals.AddRange(await matchProposalRepository.FindByReturnRouteIdsAsNoTrackingAsync(routeIds,
                cancellationToken));
        }

        if (await profilesService.FetchMerchantProfileIdByUserId(query.UserId, cancellationToken) is { } merchantId)
        {
            var requestIds = (await shipmentRequestRepository.FindByMerchantProfileIdAsNoTrackingAsync(merchantId,
                cancellationToken)).Select(request => request.Id).ToList();
            proposals.AddRange(await matchProposalRepository.FindByShipmentRequestIdsAsNoTrackingAsync(requestIds,
                cancellationToken));
        }

        return proposals
            .DistinctBy(proposal => proposal.Id)
            .OrderByDescending(proposal => proposal.CreatedAt)
            .ToList();
    }
}
