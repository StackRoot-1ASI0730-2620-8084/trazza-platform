using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

/// <summary>
///     The API identifies routes, shipment requests and proposals by their public id (Guid). This service maps them
///     to the internal ids that the rest of Matchmaking uses.
/// </summary>
public interface IPublicIdQueryService
{
    Task<ReturnRouteId?> FindReturnRouteIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<ShipmentRequestId?> FindShipmentRequestIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<MatchProposalId?> FindMatchProposalIdAsync(Guid publicId, CancellationToken cancellationToken);

    /// <summary>
    ///     Public ids of the route and the shipment request of each proposal.
    /// </summary>
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
