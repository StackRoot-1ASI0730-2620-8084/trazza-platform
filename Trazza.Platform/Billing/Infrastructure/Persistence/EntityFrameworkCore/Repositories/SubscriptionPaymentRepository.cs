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
    public async Task<IEnumerable<SubscriptionPayment>> ListBySubscriptionIdAsNoTrackingAsync(
        SubscriptionId subscriptionId, CancellationToken cancellationToken)
    {
        return await Context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(payment => payment.SubscriptionId == subscriptionId)
            .OrderByDescending(payment => payment.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
