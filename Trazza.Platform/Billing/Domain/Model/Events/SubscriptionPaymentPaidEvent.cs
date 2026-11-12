using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Domain.Model.Events;

/// <summary>
///     Raised when a subscription payment is approved by the gateway.
/// </summary>
public record SubscriptionPaymentPaidEvent(SubscriptionId SubscriptionId, Money Amount, DateTimeOffset PaidAt) : IEvent;
