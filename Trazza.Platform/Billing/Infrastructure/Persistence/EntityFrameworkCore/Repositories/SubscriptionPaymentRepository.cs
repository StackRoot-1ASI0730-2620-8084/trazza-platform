using Trazza.Platform.Billing.Domain.Model.Aggregates;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class SubscriptionPaymentRepository(AppDbContext context)
    : BaseRepository<SubscriptionPayment>(context), ISubscriptionPaymentRepository
{
    public async Task<IEnumerable<SubscriptionPayment>> FindByUserIdAsNoTrackingAsync(UserId userId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionPayment>> FindPaidByUserIdAsNoTrackingAsync(UserId userId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId && transaction.Status == ESubscriptionPaymentStatus.Paid)
            .ToListAsync(cancellationToken);
    }
}
