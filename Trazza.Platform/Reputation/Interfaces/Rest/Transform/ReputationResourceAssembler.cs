using TrazzaPlatform.API.Reputation.Domain.Services;
using TrazzaPlatform.API.Reputation.Interfaces.Rest.Resources;
using TrazzaPlatform.API.Shared.Domain.Model.ValueObjects;

namespace TrazzaPlatform.API.Reputation.Interfaces.Rest.Transform;

public static class ReputationResourceAssembler
{
    public static RatingResource ToResource(RatingView view) =>
        new(view.Rating.Id, view.TripPublicId, view.CounterpartPublicId, view.CounterpartName, view.Rating.Score,
            view.Rating.Comment, view.Rating.CreatedAt);

    public static ReputationSummaryResource ToResource(ReputationSummary summary) =>
        new(summary.UserPublicId, summary.Name, summary.AverageScore, summary.RatingsCount);

    public static LoyaltyAccountResource ToResource(LoyaltyAccount account) =>
        new(account.Balance, account.Entries.Select(entry =>
            new LoyaltyEntryResource(entry.Id, entry.Points, EnumText.ToText(entry.Reason), entry.CreatedAt)).ToList());
}