using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Repositories;

namespace Trazza.Platform.Billing.Application.Internal.QueryServices;

public class SubscriptionPaymentQueryService(ISubscriptionPaymentRepository subscriptionPaymentRepository)
    : ISubscriptionPaymentQueryService
{
    public async Task<IEnumerable<SubscriptionPayment>> Handle(GetPaymentsBySubscriptionIdQuery query,
        CancellationToken cancellationToken)
    {
        return await subscriptionPaymentRepository.ListBySubscriptionIdAsNoTrackingAsync(query.SubscriptionId,
            cancellationToken);
    }

    public async Task<SubscriptionPayment?> Handle(GetSubscriptionPaymentByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await subscriptionPaymentRepository.FindByIdAsNoTrackingAsync(query.SubscriptionPaymentId,
            cancellationToken);
    }
}
