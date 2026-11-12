namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;
 
public record PlanResource(
    string Code,
    string Name,
    string MonitoringType,
    string PricingUnit,
    decimal Price,
    string Currency,
    string BillingPeriod,
    int MinVehicles,
    int? MaxVehicles,
    bool IncludesIotDevice);
 
public record SubscribeResource(string PlanCode, List<Guid> VehicleIds);
 
public record SubscribedVehicleResource(Guid Id, string Plate);
 
public record SubscriptionPaymentResource(
    ulong Id,
    decimal Amount,
    string Currency,
    string Status,
    string? GatewayPaymentId,
    DateTime? PaidAt);
 
public record SubscriptionResource(
    ulong Id,
    PlanResource Plan,
    string Status,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    decimal MonthlyAmount,
    List<SubscribedVehicleResource> Vehicles,
    List<SubscriptionPaymentResource> Payments,
    DateTime? CancelledAt,
    DateTime CreatedAt);
 
public record BillingTripResource(Guid Id, string Status, string OriginAddress, string DestinationAddress);
 
public record MarkFreightPaidResource(string? ExternalReference);
 
public record FreightSettlementResource(
    BillingTripResource Trip,
    string MyRole,
    decimal Amount,
    string Currency,
    string Status,
    DateTime? MerchantMarkedPaidAt,
    DateTime? CarrierConfirmedAt,
    string? ExternalReference);
 
public record CancellationCompensationResource(
    BillingTripResource Trip,
    string MyRole,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    DateTime? SettledAt);