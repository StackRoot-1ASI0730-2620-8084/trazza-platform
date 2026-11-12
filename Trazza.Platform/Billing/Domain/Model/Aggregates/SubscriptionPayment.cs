using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.Events;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Errors;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Aggregates;

/// <summary>
///     Monthly charge of a subscription (table subscription_payments). Payments are simulated:
///     only the id returned by the gateway is stored, never card data.
/// </summary>
public class SubscriptionPayment : AggregateRoot
{
    public SubscriptionPayment()
    {
        Amount = new Money(0m, Money.Dollars);
    }

    public SubscriptionPayment(SubscriptionId subscriptionId, Money amount) : this()
    {
        SubscriptionId = subscriptionId;
        Amount = amount;
        Status = ESubscriptionPaymentStatus.Pending;
    }

    public SubscriptionPaymentId Id { get; }
    public SubscriptionId SubscriptionId { get; private set; }
    public Money Amount { get; private set; }
    public ESubscriptionPaymentStatus Status { get; private set; }
    public string? GatewayPaymentId { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsPending => Status == ESubscriptionPaymentStatus.Pending;

    public bool IsPaid => Status == ESubscriptionPaymentStatus.Paid;

    public Error MarkPaid(string gatewayPaymentId, DateTimeOffset now)
    {
        if (!IsPending) return BillingErrors.PaymentNotPending;
        if (string.IsNullOrWhiteSpace(gatewayPaymentId)) return BillingErrors.GatewayPaymentIdRequired;

        Status = ESubscriptionPaymentStatus.Paid;
        GatewayPaymentId = gatewayPaymentId;
        PaidAt = now;
        AddDomainEvent(new SubscriptionPaymentPaidEvent(SubscriptionId, Amount, now));
        return Error.None;
    }

    public Error MarkFailed()
    {
        if (!IsPending) return BillingErrors.PaymentNotPending;

        Status = ESubscriptionPaymentStatus.Failed;
        return Error.None;
    }

    public Error Refund()
    {
        if (!IsPaid) return BillingErrors.PaymentNotPaid;

        Status = ESubscriptionPaymentStatus.Refunded;
        return Error.None;
    }
}
