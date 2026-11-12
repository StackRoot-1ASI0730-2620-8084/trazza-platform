using Trazza.Platform.Shared.Interfaces.Rest.Resources;

namespace Trazza.Platform.Billing.Interfaces.Rest.Resources;

public record SubscriptionPaymentResource(
    ulong Id,
    ulong SubscriptionId,
    MoneyResource Amount,
    string Status,
    string? GatewayPaymentId,
    DateTimeOffset? PaidAt,
    DateTimeOffset CreatedAt);
