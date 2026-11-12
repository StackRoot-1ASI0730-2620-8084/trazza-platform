using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Billing.Interfaces.Rest.Transform;

public static class SubscriptionPaymentResourceFromEntityAssembler
{
    public static SubscriptionPaymentResource ToResourceFromEntity(SubscriptionPayment payment)
    {
        return new SubscriptionPaymentResource(
            payment.Id,
            payment.SubscriptionId,
            MoneyResourceAssembler.ToResourceFromValueObject(payment.Amount),
            payment.Status.ToSnakeCaseValue(),
            payment.GatewayPaymentId,
            payment.PaidAt,
            payment.CreatedAt);
    }
}
