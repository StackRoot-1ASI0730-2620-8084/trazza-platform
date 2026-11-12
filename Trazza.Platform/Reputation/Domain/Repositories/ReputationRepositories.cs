using TrazzaPlatform.API.Reputation.Domain.Model.Aggregates;
using TrazzaPlatform.API.Shared.Domain.Repositories;

namespace TrazzaPlatform.API.Reputation.Domain.Repositories;

public interface IRatingRepository : IBaseRepository<Rating>
{
    Task<bool> ExistsAsync(ulong tripId, ulong raterUserId, CancellationToken cancellationToken = default);

    Task<List<Rating>> ListReceivedAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<List<Rating>> ListGivenAsync(ulong userId, CancellationToken cancellationToken = default);
}

public interface ILoyaltyPointEntryRepository : IBaseRepository<LoyaltyPointEntry>
{
    Task<List<LoyaltyPointEntry>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<int> GetBalanceAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(ulong userId, ulong tripId, LoyaltyReason reason, CancellationToken cancellationToken = default);
}