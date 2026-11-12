using TrazzaPlatform.API.Billing.Domain.Model.Aggregates;
using TrazzaPlatform.API.Billing.Domain.Repositories;
using TrazzaPlatform.API.Billing.Domain.Services;
using TrazzaPlatform.API.Iam.Interfaces.Acl;
 
namespace TrazzaPlatform.API.Billing.Application.Internal;
 
public class SubscriptionViewBuilder(
    IPlanRepository planRepository,
    ISubscriptionPaymentRepository paymentRepository,
    IIamContextFacade iamContextFacade)
{
    public async Task<List<SubscriptionView>> BuildAsync(IReadOnlyCollection<Subscription> subscriptions,
        CancellationToken ct = default)
    {
        if (subscriptions.Count == 0) return [];
        var plans = (await planRepository.ListAllAsync(ct)).ToDictionary(plan => plan.Id);
        var payments = await paymentRepository.ListBySubscriptionIdsAsync(subscriptions.Select(item => item.Id).ToList(), ct);
        var vehicles = (await iamContextFacade.ListVehiclesAsync(
                subscriptions.SelectMany(item => item.Vehicles).Select(vehicle => vehicle.VehicleId).Distinct().ToList(), ct))
            .ToDictionary(vehicle => vehicle.Id);
 
        return subscriptions.Select(subscription => new SubscriptionView(
                subscription,
                plans[subscription.PlanId],
                subscription.Vehicles
                    .Select(item => vehicles.GetValueOrDefault(item.VehicleId))
                    .OfType<VehicleSnapshot>()
                    .Select(vehicle => new SubscribedVehicle(vehicle.PublicId, vehicle.Plate))
                    .ToList(),
                payments.Where(payment => payment.SubscriptionId == subscription.Id)
                    .OrderByDescending(payment => payment.CreatedAt)
                    .ToList()))
            .ToList();
    }
 
    public async Task<SubscriptionView> BuildAsync(Subscription subscription, CancellationToken ct = default) =>
        (await BuildAsync([subscription], ct))[0];
}