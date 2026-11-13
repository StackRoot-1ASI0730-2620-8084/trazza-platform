namespace Trazza.Platform.Reputation.Domain.Model.Aggregates;

public enum LoyaltyReason
{
    TripCompleted,
    RatingGiven,
    Redemption,
    Adjustment
}

public class Rating
{
    public const int MaxCommentLength = 500;

    protected Rating()
    {
    }

    public Rating(ulong tripId, ulong raterUserId, ulong ratedUserId, byte score, string? comment, DateTime now)
    {
        TripId = tripId;
        RaterUserId = raterUserId;
        RatedUserId = ratedUserId;
        Score = score;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CreatedAt = now;
    }

    public ulong Id { get; private set; }
    public ulong TripId { get; private set; }
    public ulong RaterUserId { get; private set; }
    public ulong RatedUserId { get; private set; }
    public byte Score { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
}

public class LoyaltyPointEntry
{
    public const int TripCompletedPoints = 10;
    public const int RatingGivenPoints = 2;

    protected LoyaltyPointEntry()
    {
    }

    public LoyaltyPointEntry(ulong userId, ulong? tripId, int points, LoyaltyReason reason, DateTime now)
    {
        UserId = userId;
        TripId = tripId;
        Points = points;
        Reason = reason;
        CreatedAt = now;
    }

    public ulong Id { get; private set; }
    public ulong UserId { get; private set; }
    public ulong? TripId { get; private set; }
    public int Points { get; private set; }
    public LoyaltyReason Reason { get; private set; }
    public DateTime CreatedAt { get; private set; }
}