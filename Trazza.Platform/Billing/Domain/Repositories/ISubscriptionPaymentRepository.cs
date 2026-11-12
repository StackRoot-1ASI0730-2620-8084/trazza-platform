using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Billing.Domain.Repositories;

public interface ISubscriptionPaymentRepository : IBaseRepository<SubscriptionPayment>
{
    Task<IEnumerable<SubscriptionPayment>> ListBySubscriptionIdAsNoTrackingAsync(SubscriptionId subscriptionId,
        CancellationToken cancellationToken);
}
