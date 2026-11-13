using Trazza.Platform.Reputation.Domain.Model.Aggregates;
using Trazza.Platform.Reputation.Domain.Model.Commands;
using Trazza.Platform.Shared.Domain.Model;

namespace Trazza.Platform.Reputation.Domain.Services;

public record RatingView(Rating Rating, Guid TripPublicId, Guid CounterpartPublicId, string CounterpartName);

public record ReputationSummary(Guid UserPublicId, string Name, double? AverageScore, int RatingsCount);

public record LoyaltyAccount(int Balance, List<LoyaltyPointEntry> Entries);

public interface IReputationCommandService
{
    Task<Result<RatingView, CommandError>> Handle(RateTripCommand command, CancellationToken ct = default);

    Task<Result<LoyaltyAccount, CommandError>> Handle(RedeemPointsCommand command, CancellationToken ct = default);
}

public interface IReputationQueryService
{
    Task<List<RatingView>> ListReceivedAsync(ulong userId, CancellationToken ct = default);

    Task<List<RatingView>> ListGivenAsync(ulong userId, CancellationToken ct = default);

    Task<Result<ReputationSummary, CommandError>> GetSummaryAsync(Guid userPublicId, CancellationToken ct = default);

    Task<LoyaltyAccount> GetLoyaltyAccountAsync(ulong userId, CancellationToken ct = default);
}