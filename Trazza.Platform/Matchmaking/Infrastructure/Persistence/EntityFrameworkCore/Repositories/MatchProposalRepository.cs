using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class MatchProposalRepository(AppDbContext context)
    : BaseRepository<MatchProposal>(context), IMatchProposalRepository
{
    public async Task<IEnumerable<MatchProposal>> FindByShipmentRequestIdAsNoTrackingAsync(
        ShipmentRequestId shipmentRequestId, CancellationToken cancellationToken)
    {
        return await Context.Set<MatchProposal>()
            .AsNoTracking()
            .Where(proposal => proposal.ShipmentRequestId == shipmentRequestId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MatchProposal>> FindByReturnRouteIdAsNoTrackingAsync(ReturnRouteId returnRouteId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<MatchProposal>()
            .AsNoTracking()
            .Where(proposal => proposal.ReturnRouteId == returnRouteId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ShipmentRequestId shipmentRequestId, ReturnRouteId returnRouteId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<MatchProposal>()
            .AnyAsync(proposal => proposal.ShipmentRequestId == shipmentRequestId &&
                                  proposal.ReturnRouteId == returnRouteId, cancellationToken);
    }

    public async Task<IEnumerable<MatchProposal>> ListPendingByShipmentRequestIdAsync(
        ShipmentRequestId shipmentRequestId, CancellationToken cancellationToken)
    {
        return await Context.Set<MatchProposal>()
            .Where(proposal => proposal.ShipmentRequestId == shipmentRequestId &&
                               proposal.Status == EMatchProposalStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<(int Shipments, decimal WeightKg)> GetAcceptedLoadAsync(ReturnRouteId returnRouteId,
        CancellationToken cancellationToken)
    {
        var weights = await (from proposal in Context.Set<MatchProposal>()
                join shipment in Context.Set<ShipmentRequest>() on proposal.ShipmentRequestId equals shipment.Id
                where proposal.ReturnRouteId == returnRouteId && proposal.Status == EMatchProposalStatus.Accepted
                select shipment.Cargo.WeightKg)
            .ToListAsync(cancellationToken);

        return (weights.Count, weights.Sum());
    }
}
