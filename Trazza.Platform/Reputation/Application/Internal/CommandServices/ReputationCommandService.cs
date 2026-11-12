using TrazzaPlatform.API.Execution.Interfaces.Acl;
using TrazzaPlatform.API.Reputation.Domain.Model.Aggregates;
using TrazzaPlatform.API.Reputation.Domain.Model.Commands;
using TrazzaPlatform.API.Reputation.Domain.Repositories;
using TrazzaPlatform.API.Reputation.Domain.Services;
using TrazzaPlatform.API.Shared.Domain.Model;
using TrazzaPlatform.API.Shared.Domain.Repositories;

namespace TrazzaPlatform.API.Reputation.Application.Internal.CommandServices;

/// <summary>
///     Ratings of confirmed trips (+2 points for rating) and redemption of loyalty points.
/// </summary>
public class ReputationCommandService(
    IRatingRepository ratingRepository,
    ILoyaltyPointEntryRepository loyaltyRepository,
    IExecutionContextFacade executionContextFacade,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    RatingViewBuilder viewBuilder) : IReputationCommandService
{
    public async Task<Result<RatingView, CommandError>> Handle(RateTripCommand command, CancellationToken ct = default)
    {
        var trip = await executionContextFacade.FindTripAsync(command.TripId, ct);
        if (trip is null || (trip.CarrierUserId != command.UserId && trip.MerchantUserId != command.UserId))
            return CommandError.NotFound("trip-not-found");
        if (trip.Status != "CONFIRMED") return CommandError.Conflict("trip-not-confirmed");
        if (command.Score is < 1 or > 5) return CommandError.Validation("score-invalid");
        if (command.Comment?.Trim().Length > Rating.MaxCommentLength) return CommandError.Validation("comment-invalid");
        if (await ratingRepository.ExistsAsync(trip.Id, command.UserId, ct))
            return CommandError.Conflict("rating-already-given");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var ratedUserId = command.UserId == trip.CarrierUserId ? trip.MerchantUserId : trip.CarrierUserId;
        var rating = new Rating(trip.Id, command.UserId, ratedUserId, (byte)command.Score, command.Comment, now);
        await ratingRepository.AddAsync(rating, ct);
        await loyaltyRepository.AddAsync(new LoyaltyPointEntry(command.UserId, trip.Id, LoyaltyPointEntry.RatingGivenPoints,
            LoyaltyReason.RatingGiven, now), ct);
        await unitOfWork.CompleteAsync(ct);
        return (await viewBuilder.BuildAsync([rating], command.UserId, ct))[0];
    }

    public Task<Result<LoyaltyAccount, CommandError>> Handle(RedeemPointsCommand command, CancellationToken ct = default) =>
        unitOfWork.ExecuteInTransactionAsync(() => RedeemAsync(command, ct), result => result.IsSuccess, ct);

    private async Task<Result<LoyaltyAccount, CommandError>> RedeemAsync(RedeemPointsCommand command, CancellationToken ct)
    {
        if (command.Points <= 0) return CommandError.Validation("points-invalid");
        var balance = await loyaltyRepository.GetBalanceAsync(command.UserId, ct);
        if (command.Points > balance) return CommandError.Conflict("points-insufficient");

        await loyaltyRepository.AddAsync(new LoyaltyPointEntry(command.UserId, null, -command.Points,
            LoyaltyReason.Redemption, timeProvider.GetUtcNow().UtcDateTime), ct);
        await unitOfWork.CompleteAsync(ct);
        return new LoyaltyAccount(await loyaltyRepository.GetBalanceAsync(command.UserId, ct),
            await loyaltyRepository.ListByUserAsync(command.UserId, ct));
    }
}