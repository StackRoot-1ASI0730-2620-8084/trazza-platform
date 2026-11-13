using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Domain.Repositories;

public interface IMatchProposalRepository : IBaseRepository<MatchProposal>
{
    Task<IEnumerable<MatchProposal>> FindByShipmentRequestIdAsNoTrackingAsync(ShipmentRequestId shipmentRequestId,
        CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> FindByReturnRouteIdAsNoTrackingAsync(ReturnRouteId returnRouteId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     There is only one proposal for each shipment request and route pair.
    /// </summary>
    Task<bool> ExistsAsync(ShipmentRequestId shipmentRequestId, ReturnRouteId returnRouteId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Pending proposals of a shipment request, tracked, to expire them when one is accepted.
    /// </summary>
    Task<IEnumerable<MatchProposal>> ListPendingByShipmentRequestIdAsync(ShipmentRequestId shipmentRequestId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Number of accepted shipments of a route and their total weight in kilograms.
    /// </summary>
    Task<(int Shipments, decimal WeightKg)> GetAcceptedLoadAsync(ReturnRouteId returnRouteId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Id of the proposal with the given public id, or null when there is none.
    /// </summary>
    Task<MatchProposalId?> FindIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    /// <summary>
    ///     Public ids of the given entities, by id. It reads only the two id columns.
    /// </summary>
    Task<IReadOnlyDictionary<ulong, Guid>> FindPublicIdsAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken);
}
