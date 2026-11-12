using TrazzaPlatform.API.Billing.Domain.Model.ValueObjects;
 
namespace TrazzaPlatform.API.Billing.Domain.Model.Aggregates;

public class Subscription
{
    private readonly List<SubscriptionVehicle> _vehicles = [];
 
    protected Subscription()
    {
    }
 
    public Subscription(ulong userId, ushort planId, IEnumerable<ulong> vehicleIds, string gatewaySubscriptionId,
        DateTime now)
    {
        UserId = userId;
        PlanId = planId;
        Status = SubscriptionStatus.Active;
        CurrentPeriodStart = now;
        CurrentPeriodEnd = now.AddMonths(1);
        GatewaySubscriptionId = gatewaySubscriptionId;
        CreatedAt = now;
        foreach (var vehicleId in vehicleIds.Distinct()) _vehicles.Add(new SubscriptionVehicle(vehicleId, now));
    }
 
    public ulong Id { get; private set; }
    public ulong UserId { get; private set; }
    public ushort PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public string? GatewaySubscriptionId { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<SubscriptionVehicle> Vehicles => _vehicles;
 
    public bool IsActive => Status == SubscriptionStatus.Active;
 
    public void Renew()
    {
        CurrentPeriodStart = CurrentPeriodEnd;
        CurrentPeriodEnd = CurrentPeriodEnd.AddMonths(1);
        Status = SubscriptionStatus.Active;
    }
 
    public void Cancel(DateTime now)
    {
        if (Status == SubscriptionStatus.Cancelled) return;
        Status = SubscriptionStatus.Cancelled;
        CancelledAt = now;
    }
}
 
public class SubscriptionVehicle
{
    protected SubscriptionVehicle()
    {
    }
 
    public SubscriptionVehicle(ulong vehicleId, DateTime addedAt)
    {
        VehicleId = vehicleId;
        AddedAt = addedAt;
    }
 
    public ulong SubscriptionId { get; private set; }
    public ulong VehicleId { get; private set; }
    public DateTime AddedAt { get; private set; }
}
 
public class SubscriptionPayment
{
    protected SubscriptionPayment()
    {
        Currency = string.Empty;
    }
 
    public static SubscriptionPayment Simulated(ulong subscriptionId, decimal amount, string currency, DateTime now) =>
        new()
        {
            SubscriptionId = subscriptionId,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatus.Paid,
            GatewayPaymentId = $"SIM-PAY-{Guid.CreateVersion7():N}",
            PaidAt = now,
            CreatedAt = now
        };
 
    public ulong Id { get; private set; }
    public ulong SubscriptionId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? GatewayPaymentId { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
