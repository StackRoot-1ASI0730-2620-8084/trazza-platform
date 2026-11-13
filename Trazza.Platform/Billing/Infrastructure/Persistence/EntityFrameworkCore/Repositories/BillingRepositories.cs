using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
 
namespace Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
 
public class PlanRepository(AppDbContext context) : IPlanRepository
{
    public async Task<List<Plan>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await context.Set<Plan>().AsNoTracking().Where(plan => plan.IsActive).OrderBy(plan => plan.Price)
            .ToListAsync(cancellationToken);
 
    public async Task<List<Plan>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await context.Set<Plan>().AsNoTracking().ToListAsync(cancellationToken);
 
    public async Task<Plan?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await context.Set<Plan>().AsNoTracking().FirstOrDefaultAsync(plan => plan.Code == code, cancellationToken);
}
 
public class SubscriptionRepository(AppDbContext context)
    : BaseRepository<Subscription>(context), ISubscriptionRepository
{
    public async Task<List<Subscription>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await Context.Set<Subscription>()
            .Include(subscription => subscription.Vehicles)
            .Where(subscription => subscription.UserId == userId)
            .OrderByDescending(subscription => subscription.CreatedAt)
            .ToListAsync(cancellationToken);
 
    public async Task<Subscription?> FindWithVehiclesAsync(ulong id, CancellationToken cancellationToken = default) =>
        await Context.Set<Subscription>()
            .Include(subscription => subscription.Vehicles)
            .FirstOrDefaultAsync(subscription => subscription.Id == id, cancellationToken);
 
    public async Task<List<ulong>> ListSubscribedVehicleIdsAsync(IReadOnlyCollection<ulong> vehicleIds,
        CancellationToken cancellationToken = default)
    {
        if (vehicleIds.Count == 0) return [];
        return await Context.Set<Subscription>()
            .Where(subscription => subscription.Status == SubscriptionStatus.Active)
            .SelectMany(subscription => subscription.Vehicles)
            .Where(vehicle => vehicleIds.Contains(vehicle.VehicleId))
            .Select(vehicle => vehicle.VehicleId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
 
public class SubscriptionPaymentRepository(AppDbContext context)
    : BaseRepository<SubscriptionPayment>(context), ISubscriptionPaymentRepository
{
    public async Task<List<SubscriptionPayment>> ListBySubscriptionIdsAsync(IReadOnlyCollection<ulong> subscriptionIds,
        CancellationToken cancellationToken = default)
    {
        if (subscriptionIds.Count == 0) return [];
        return await Context.Set<SubscriptionPayment>()
            .Where(payment => subscriptionIds.Contains(payment.SubscriptionId))
            .ToListAsync(cancellationToken);
    }
}
 
public class FreightSettlementRepository(AppDbContext context)
    : BaseRepository<FreightSettlement>(context), IFreightSettlementRepository
{
    public async Task<List<FreightSettlement>> ListByTripIdsAsync(IReadOnlyCollection<ulong> tripIds,
        CancellationToken cancellationToken = default)
    {
        if (tripIds.Count == 0) return [];
        return await Context.Set<FreightSettlement>()
            .Where(settlement => tripIds.Contains(settlement.TripId))
            .ToListAsync(cancellationToken);
    }
}
 
public class CancellationCompensationRepository(AppDbContext context)
    : BaseRepository<CancellationCompensation>(context), ICancellationCompensationRepository
{
    public async Task<List<CancellationCompensation>> ListByTripIdsAsync(IReadOnlyCollection<ulong> tripIds,
        CancellationToken cancellationToken = default)
    {
        if (tripIds.Count == 0) return [];
        return await Context.Set<CancellationCompensation>()
            .Where(compensation => tripIds.Contains(compensation.TripId))
            .ToListAsync(cancellationToken);
    }
}
