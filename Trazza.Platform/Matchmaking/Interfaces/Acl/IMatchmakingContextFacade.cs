namespace Trazza.Platform.Matchmaking.Interfaces.Acl;

/// <summary>
///     What other contexts (for example Execution) need to know about an accepted match.
///     It only uses primitive types, so they do not depend on the Matchmaking model.
/// </summary>
public record AcceptedMatchSummary(
    ulong MatchProposalId,
    ulong ShipmentRequestId,
    ulong ReturnRouteId,
    ulong CarrierProfileId,
    ulong MerchantProfileId,
    ulong VehicleId,
    ulong? DriverId,
    decimal OfferedAmount,
    string Currency);

public interface IMatchmakingContextFacade
{
    /// <summary>
    ///     Returns the data of an accepted proposal, or null when it does not exist or it is not accepted.
    /// </summary>
    Task<AcceptedMatchSummary?> FetchAcceptedMatch(ulong matchProposalId, CancellationToken cancellationToken);
}
