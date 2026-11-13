using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Application.Internal.QueryServices;

public class PublicIdQueryService(
    IReturnRouteRepository returnRouteRepository,
    IShipmentRequestRepository shipmentRequestRepository,
    IMatchProposalRepository matchProposalRepository)
    : IPublicIdQueryService
{
    public Task<ReturnRouteId?> FindReturnRouteIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        returnRouteRepository.FindIdByPublicIdAsync(publicId, cancellationToken);

    public Task<ShipmentRequestId?> FindShipmentRequestIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        shipmentRequestRepository.FindIdByPublicIdAsync(publicId, cancellationToken);

    public Task<MatchProposalId?> FindMatchProposalIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        matchProposalRepository.FindIdByPublicIdAsync(publicId, cancellationToken);

    public async Task<ProposalPublicIds> FetchProposalPublicIdsAsync(IEnumerable<MatchProposal> proposals,
        CancellationToken cancellationToken)
    {
        var list = proposals.ToList();
        if (list.Count == 0) return ProposalPublicIds.Empty;

        var routes = await returnRouteRepository.FindPublicIdsAsync(
            list.Select(proposal => proposal.ReturnRouteId.Value).ToList(), cancellationToken);
        var shipments = await shipmentRequestRepository.FindPublicIdsAsync(
            list.Select(proposal => proposal.ShipmentRequestId.Value).ToList(), cancellationToken);
        return new ProposalPublicIds(routes, shipments);
    }
}
