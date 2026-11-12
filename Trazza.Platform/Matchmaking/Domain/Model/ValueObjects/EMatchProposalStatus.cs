namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

/// <summary>
///     Status of a match proposal. Stored as PENDING, ACCEPTED, REJECTED, EXPIRED or CANCELLED.
/// </summary>
public enum EMatchProposalStatus
{
    Pending,
    Accepted,
    Rejected,
    Expired,
    Cancelled
}
