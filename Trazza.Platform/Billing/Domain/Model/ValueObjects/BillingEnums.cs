namespace TrazzaPlatform.API.Billing.Domain.Model.ValueObjects;
 
public enum MonitoringType
{
    Route,
    IotCargo,
    Full
}
 
public enum PricingUnit
{
    PerVehicle,
    Package
}
 
public enum BillingPeriod
{
    Monthly
}
 
public enum SubscriptionStatus
{
    Active,
    PastDue,
    Cancelled
}
 
public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}
 
public enum CompensationStatus
{
    Pending,
    PaidExternally,
    Waived,
    Disputed
}

public enum SettlementStatus
{
    Pending,
    MarkedPaid,
    Settled
}