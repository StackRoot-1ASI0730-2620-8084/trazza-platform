using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

/// <summary>
///     Identifier of a subscription payment (subscription_payments.id, BIGINT UNSIGNED).
/// </summary>
public readonly record struct SubscriptionPaymentId(ulong Value) : IEntityId<ulong>
{
    public static implicit operator ulong(SubscriptionPaymentId id) => id.Value;
    public static implicit operator SubscriptionPaymentId(ulong value) => new(value);

    public override string ToString() => Value.ToString();
}
