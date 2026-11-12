namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

/// <summary>
///     Status of a return route. Stored as PUBLISHED, FULL, IN_PROGRESS, COMPLETED or CANCELLED.
/// </summary>
public enum EReturnRouteStatus
{
    Published,
    Full,
    InProgress,
    Completed,
    Cancelled
}
