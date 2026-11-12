namespace TrazzaPlatform.API.Reputation.Interfaces.Rest.Resources;

public record RateTripResource(Guid TripId, int Score, string? Comment);

public record RatingResource(
    ulong Id,
    Guid TripId,
    Guid CounterpartId,
    string CounterpartName,
    int Score,
    string? Comment,
    DateTime CreatedAt);

public record ReputationSummaryResource(Guid UserId, string Name, double? AverageScore, int RatingsCount);

public record LoyaltyEntryResource(ulong Id, int Points, string Reason, DateTime CreatedAt);

public record LoyaltyAccountResource(int Balance, List<LoyaltyEntryResource> Entries);

public record RedeemPointsResource(int Points);