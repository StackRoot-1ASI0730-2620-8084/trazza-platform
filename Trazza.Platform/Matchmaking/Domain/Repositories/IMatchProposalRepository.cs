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

    Task<bool> ExistsAsync(ShipmentRequestId shipmentRequestId, ReturnRouteId returnRouteId,
        CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> ListPendingByShipmentRequestIdAsync(ShipmentRequestId shipmentRequestId,
        CancellationToken cancellationToken);

    Task<(int Shipments, decimal WeightKg)> GetAcceptedLoadAsync(ReturnRouteId returnRouteId,
        CancellationToken cancellationToken);

    Task<MatchProposalId?> FindIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<ulong, Guid>> FindPublicIdsAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> FindByReturnRouteIdsAsNoTrackingAsync(IReadOnlyCollection<ReturnRouteId> ids,
        CancellationToken cancellationToken);

    Task<IEnumerable<MatchProposal>> FindByShipmentRequestIdsAsNoTrackingAsync(
        IReadOnlyCollection<ShipmentRequestId> ids, CancellationToken cancellationToken);
}
