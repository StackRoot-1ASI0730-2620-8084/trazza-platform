using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Domain.Repositories;
 
namespace Trazza.Platform.Billing.Domain.Repositories;
 
public interface IPlanRepository
{
    Task<List<Plan>> ListActiveAsync(CancellationToken cancellationToken = default);
 
    Task<List<Plan>> ListAllAsync(CancellationToken cancellationToken = default);
 
    Task<Plan?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
 
public interface ISubscriptionRepository : IBaseRepository<Subscription>
{
    Task<List<Subscription>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default);
 
    Task<Subscription?> FindWithVehiclesAsync(ulong id, CancellationToken cancellationToken = default);
 
    Task<List<ulong>> ListSubscribedVehicleIdsAsync(IReadOnlyCollection<ulong> vehicleIds,
        CancellationToken cancellationToken = default);
}
 
public interface ISubscriptionPaymentRepository : IBaseRepository<SubscriptionPayment>
{
    Task<List<SubscriptionPayment>> ListBySubscriptionIdsAsync(IReadOnlyCollection<ulong> subscriptionIds,
        CancellationToken cancellationToken = default);
}
 
public interface IFreightSettlementRepository : IBaseRepository<FreightSettlement>
{
    Task<List<FreightSettlement>> ListByTripIdsAsync(IReadOnlyCollection<ulong> tripIds,
        CancellationToken cancellationToken = default);
}
 
public interface ICancellationCompensationRepository : IBaseRepository<CancellationCompensation>
{
    Task<List<CancellationCompensation>> ListByTripIdsAsync(IReadOnlyCollection<ulong> tripIds,
        CancellationToken cancellationToken = default);
}