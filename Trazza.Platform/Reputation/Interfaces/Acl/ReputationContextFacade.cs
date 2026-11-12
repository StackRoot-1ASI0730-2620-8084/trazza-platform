using TrazzaPlatform.API.Reputation.Domain.Model.Aggregates;
using TrazzaPlatform.API.Reputation.Domain.Repositories;

namespace TrazzaPlatform.API.Reputation.Interfaces.Acl;

public class ReputationContextFacade(ILoyaltyPointEntryRepository loyaltyRepository) : IReputationContextFacade
{
    public async Task AwardTripCompletedAsync(ulong userId, ulong tripId, DateTime now, CancellationToken ct = default)
    {
        if (userId == 0 || await loyaltyRepository.ExistsAsync(userId, tripId, LoyaltyReason.TripCompleted, ct)) return;
        await loyaltyRepository.AddAsync(new LoyaltyPointEntry(userId, tripId, LoyaltyPointEntry.TripCompletedPoints,
            LoyaltyReason.TripCompleted, now), ct);
    }
}