namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

/// <summary>
///     Status of a subscription payment. Stored as PENDING, PAID, FAILED or REFUNDED.
/// </summary>
public enum ESubscriptionPaymentStatus
{
    Pending,
    Paid,
    Failed,
    Refunded
}
