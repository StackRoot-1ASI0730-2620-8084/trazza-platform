using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Billing.Application.Internal.QueryServices;

public class SubscriptionPaymentQueryService(ISubscriptionPaymentRepository paymentTransactionRepository)
    : ISubscriptionPaymentQueryService
{
    public async Task<IEnumerable<SubscriptionPayment>> Handle(GetAllPaymentTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        return await paymentTransactionRepository.ListAsNoTrackingAsync(cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionPayment>> Handle(GetPaymentsBySubscriptionIdQuery query,
        CancellationToken cancellationToken)
    {
        return await paymentTransactionRepository.FindByUserIdAsNoTrackingAsync(query.UserId, cancellationToken);
    }

    public async Task<SubscriptionPayment?> Handle(GetSubscriptionPaymentByIdQuery query,
        CancellationToken cancellationToken)
    {
        return await paymentTransactionRepository.FindByIdAsNoTrackingAsync(query.SubscriptionPaymentId,
            cancellationToken);
    }

    public async Task<SubscriptionPayment?> Handle(GetActiveSubscriptionsByUserIdQuery query,
        CancellationToken cancellationToken)
    {
        var paidTransactions =
            await paymentTransactionRepository.FindPaidByUserIdAsNoTrackingAsync(query.UserId, cancellationToken);
        var today = LimaCalendar.Today;
        return paidTransactions
            .Where(transaction => transaction.Covers(today))
            .OrderByDescending(transaction => transaction.PeriodEnd)
            .FirstOrDefault();
    }
}
