using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.ValueObjects;

public readonly record struct SubscriptionPaymentId(int Value) : IEntityId<int>
{
    public static implicit operator int(SubscriptionPaymentId id) => id.Value;
    public static implicit operator SubscriptionPaymentId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
