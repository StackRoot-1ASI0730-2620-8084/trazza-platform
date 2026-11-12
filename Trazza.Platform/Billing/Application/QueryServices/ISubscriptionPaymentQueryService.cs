using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Queries;

namespace Trazza.Platform.Billing.Application.QueryServices;

public interface ISubscriptionPaymentQueryService
{
    Task<IEnumerable<SubscriptionPayment>> Handle(GetPaymentsBySubscriptionIdQuery query,
        CancellationToken cancellationToken);

    Task<SubscriptionPayment?> Handle(GetSubscriptionPaymentByIdQuery query, CancellationToken cancellationToken);
}
