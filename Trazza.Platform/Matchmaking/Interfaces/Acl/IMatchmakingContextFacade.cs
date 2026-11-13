using Trazza.Platform.Execution.Domain.Services;

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

    /// <summary>
    ///     Returns the ids of the accepted or withdrawn proposals of the carrier routes or the merchant requests.
    /// </summary>
    Task<List<ulong>> ListProposalIdsWithTripsAsync(ulong? carrierProfileId, ulong? merchantProfileId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the route and shipment of each proposal. Proposals that no longer exist are skipped.
    /// </summary>
    Task<List<TripContextSnapshot>> ListTripContextsAsync(IReadOnlyCollection<ulong> matchProposalIds,
        CancellationToken cancellationToken);

    /// <summary>
    ///     The cargo was picked up: the shipment request is in transit and the route is in progress.
    /// </summary>
    Task MarkPickedUpAsync(ulong matchProposalId, CancellationToken cancellationToken);

    /// <summary>
    ///     The cargo was delivered: the shipment request is delivered and the route is completed when nothing else is
    ///     pending on it.
    /// </summary>
    Task MarkDeliveredAsync(ulong matchProposalId, CancellationToken cancellationToken);

    /// <summary>
    ///     The trip was cancelled: the proposal is withdrawn, the shipment request is cancelled (by the merchant, or
    ///     when its pickup window is over) or opened again, and the place on the route is released.
    /// </summary>
    Task ReleaseAsync(ulong matchProposalId, bool cancelledByMerchant, string reason, DateTime now,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes the chat of the proposal once its trip is closed.
    /// </summary>
    Task<int> PurgeChatAsync(ulong matchProposalId, CancellationToken cancellationToken);
}
