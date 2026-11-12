namespace TrazzaPlatform.API.Reputation.Domain.Model.Commands;

public record RateTripCommand(ulong UserId, Guid TripId, int Score, string? Comment);

public record RedeemPointsCommand(ulong UserId, int Points);