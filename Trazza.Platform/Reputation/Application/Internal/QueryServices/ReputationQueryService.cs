using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Reputation.Domain.Repositories;
using Trazza.Platform.Reputation.Domain.Services;
using Trazza.Platform.Shared.Domain.Model;

namespace Trazza.Platform.Reputation.Application.Internal.QueryServices;

public class ReputationQueryService(
    IRatingRepository ratingRepository,
    ILoyaltyPointEntryRepository loyaltyRepository,
    IIamContextFacade iamContextFacade,
    RatingViewBuilder viewBuilder) : IReputationQueryService
{
    public async Task<List<RatingView>> ListReceivedAsync(ulong userId, CancellationToken ct = default) =>
        await viewBuilder.BuildAsync(await ratingRepository.ListReceivedAsync(userId, ct), userId, ct);

    public async Task<List<RatingView>> ListGivenAsync(ulong userId, CancellationToken ct = default) =>
        await viewBuilder.BuildAsync(await ratingRepository.ListGivenAsync(userId, ct), userId, ct);

    public async Task<Result<ReputationSummary, CommandError>> GetSummaryAsync(Guid userPublicId,
        CancellationToken ct = default)
    {
        var userId = await iamContextFacade.FindUserIdByPublicIdAsync(userPublicId, ct);
        if (userId is null) return CommandError.NotFound("user-not-found");
        var contact = await iamContextFacade.FindContactAsync(userId.Value, ct);
        var ratings = await ratingRepository.ListReceivedAsync(userId.Value, ct);
        double? average = ratings.Count == 0 ? null : Math.Round(ratings.Average(rating => rating.Score), 2);
        return new ReputationSummary(userPublicId, contact?.BusinessName ?? contact?.FullName ?? string.Empty, average,
            ratings.Count);
    }

    public async Task<LoyaltyAccount> GetLoyaltyAccountAsync(ulong userId, CancellationToken ct = default) =>
        new(await loyaltyRepository.GetBalanceAsync(userId, ct), await loyaltyRepository.ListByUserAsync(userId, ct));
}