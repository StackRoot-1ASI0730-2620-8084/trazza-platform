using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Billing.Domain.Services;
using Trazza.Platform.Execution.Interfaces.Acl;
 
namespace Trazza.Platform.Billing.Application.Internal.QueryServices;
 
public class BillingQueryService(
    IPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository,
    IFreightSettlementRepository settlementRepository,
    ICancellationCompensationRepository compensationRepository,
    IExecutionContextFacade executionContextFacade,
    SubscriptionViewBuilder viewBuilder) : IBillingQueryService
{
    public Task<List<Plan>> ListPlansAsync(CancellationToken ct = default) => planRepository.ListActiveAsync(ct);
 
    public async Task<List<SubscriptionView>> ListMySubscriptionsAsync(ulong userId, CancellationToken ct = default) =>
        await viewBuilder.BuildAsync(await subscriptionRepository.ListByUserAsync(userId, ct), ct);
 
    public async Task<List<FreightSettlementView>> ListMySettlementsAsync(ulong userId, CancellationToken ct = default)
    {
        var trips = (await executionContextFacade.ListTripsOfUserAsync(userId, ct)).ToDictionary(trip => trip.Id);
        return (await settlementRepository.ListByTripIdsAsync(trips.Keys.ToList(), ct))
            .Select(settlement => new FreightSettlementView(settlement, trips[settlement.TripId],
                trips[settlement.TripId].CarrierUserId == userId))
            .OrderByDescending(view => view.Trip.DeliveredAt)
            .ToList();
    }
 
    public async Task<List<CompensationView>> ListMyCompensationsAsync(ulong userId, CancellationToken ct = default)
    {
        var trips = (await executionContextFacade.ListTripsOfUserAsync(userId, ct)).ToDictionary(trip => trip.Id);
        return (await compensationRepository.ListByTripIdsAsync(trips.Keys.ToList(), ct))
            .Select(compensation => new CompensationView(compensation, trips[compensation.TripId],
                trips[compensation.TripId].CarrierUserId == userId))
            .OrderByDescending(view => view.Compensation.CreatedAt)
            .ToList();
    }
}
