using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class RatingRepository(AppDbContext context) : BaseRepository<Rating>(context), IRatingRepository
{
    public async Task<bool> ExistsAsync(ulong tripId, ulong raterUserId, CancellationToken cancellationToken = default) =>
        await Context.Set<Rating>().AnyAsync(rating => rating.TripId == tripId && rating.RaterUserId == raterUserId,
            cancellationToken);

    public async Task<List<Rating>> ListReceivedAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await Context.Set<Rating>().Where(rating => rating.RatedUserId == userId)
            .OrderByDescending(rating => rating.CreatedAt).ToListAsync(cancellationToken);

    public async Task<List<Rating>> ListGivenAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await Context.Set<Rating>().Where(rating => rating.RaterUserId == userId)
            .OrderByDescending(rating => rating.CreatedAt).ToListAsync(cancellationToken);
}

public class LoyaltyPointEntryRepository(AppDbContext context)
    : BaseRepository<LoyaltyPointEntry>(context), ILoyaltyPointEntryRepository
{
    public async Task<List<LoyaltyPointEntry>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await Context.Set<LoyaltyPointEntry>().Where(entry => entry.UserId == userId)
            .OrderByDescending(entry => entry.CreatedAt).ToListAsync(cancellationToken);

    public async Task<int> GetBalanceAsync(ulong userId, CancellationToken cancellationToken = default) =>
        await Context.Set<LoyaltyPointEntry>().Where(entry => entry.UserId == userId)
            .SumAsync(entry => entry.Points, cancellationToken);

    public async Task<bool> ExistsAsync(ulong userId, ulong tripId, LoyaltyReason reason,
        CancellationToken cancellationToken = default) =>
        await Context.Set<LoyaltyPointEntry>().AnyAsync(entry => entry.UserId == userId && entry.TripId == tripId
                                                                 && entry.Reason == reason, cancellationToken);
}