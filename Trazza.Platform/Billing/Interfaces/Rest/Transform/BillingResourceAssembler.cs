using TrazzaPlatform.API.Billing.Domain.Model.Aggregates;
using TrazzaPlatform.API.Billing.Domain.Services;
using TrazzaPlatform.API.Billing.Interfaces.Rest.Resources;
using TrazzaPlatform.API.Execution.Interfaces.Acl;
using TrazzaPlatform.API.Iam.Domain.Model.ValueObjects;
using TrazzaPlatform.API.Shared.Domain.Model.ValueObjects;
 
namespace TrazzaPlatform.API.Billing.Interfaces.Rest.Transform;
 
public static class BillingResourceAssembler
{
    public static PlanResource ToResource(Plan plan) =>
        new(plan.Code, plan.Name, EnumText.ToText(plan.MonitoringType), EnumText.ToText(plan.PricingUnit), plan.Price,
            plan.Currency, EnumText.ToText(plan.BillingPeriod), plan.MinVehicles, plan.MaxVehicles, plan.IncludesIot);
 
    public static SubscriptionResource ToResource(SubscriptionView view) =>
        new(view.Subscription.Id,
            ToResource(view.Plan),
            EnumText.ToText(view.Subscription.Status),
            view.Subscription.CurrentPeriodStart,
            view.Subscription.CurrentPeriodEnd,
            view.Plan.PriceFor(view.Subscription.Vehicles.Count),
            view.Vehicles.Select(vehicle => new SubscribedVehicleResource(vehicle.Id, vehicle.Plate)).ToList(),
            view.Payments.Select(payment => new SubscriptionPaymentResource(payment.Id, payment.Amount, payment.Currency,
                EnumText.ToText(payment.Status), payment.GatewayPaymentId, payment.PaidAt)).ToList(),
            view.Subscription.CancelledAt,
            view.Subscription.CreatedAt);
 
    public static FreightSettlementResource ToResource(FreightSettlementView view) =>
        new(ToResource(view.Trip), view.IsCarrier ? Roles.Carrier : Roles.Merchant, view.Settlement.Amount,
            view.Settlement.Currency, EnumText.ToText(view.Settlement.Status), view.Settlement.MerchantMarkedPaidAt,
            view.Settlement.CarrierConfirmedAt, view.Settlement.ExternalReference);
 
    public static CancellationCompensationResource ToResource(CompensationView view) =>
        new(ToResource(view.Trip), view.IsCarrier ? Roles.Carrier : Roles.Merchant, view.Compensation.Amount,
            view.Compensation.Currency, EnumText.ToText(view.Compensation.Status), view.Compensation.CreatedAt,
            view.Compensation.SettledAt);
 
    private static BillingTripResource ToResource(TripSnapshot trip) =>
        new(trip.PublicId, trip.Status, trip.OriginAddress, trip.DestinationAddress);
}
