using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IPublicIdQueryService
{
    Task<ReturnRouteId?> FindReturnRouteIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<ShipmentRequestId?> FindShipmentRequestIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<MatchProposalId?> FindMatchProposalIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<ProposalPublicIds> FetchProposalPublicIdsAsync(IEnumerable<MatchProposal> proposals,
        CancellationToken cancellationToken);
}

public record ProposalPublicIds(
    IReadOnlyDictionary<ulong, Guid> ReturnRoutes,
    IReadOnlyDictionary<ulong, Guid> ShipmentRequests)
{
    public static readonly ProposalPublicIds Empty =
        new(new Dictionary<ulong, Guid>(), new Dictionary<ulong, Guid>());
}
