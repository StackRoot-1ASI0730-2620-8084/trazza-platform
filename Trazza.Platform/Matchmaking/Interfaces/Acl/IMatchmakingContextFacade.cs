using Trazza.Platform.Execution.Domain.Services;

namespace Trazza.Platform.Matchmaking.Interfaces.Acl;

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
    Task<AcceptedMatchSummary?> FetchAcceptedMatch(ulong matchProposalId, CancellationToken cancellationToken);

    Task<List<ulong>> ListProposalIdsWithTripsAsync(ulong? carrierProfileId, ulong? merchantProfileId,
        CancellationToken cancellationToken);

    Task<List<TripContextSnapshot>> ListTripContextsAsync(IReadOnlyCollection<ulong> matchProposalIds,
        CancellationToken cancellationToken);

    Task MarkPickedUpAsync(ulong matchProposalId, CancellationToken cancellationToken);

    Task MarkDeliveredAsync(ulong matchProposalId, CancellationToken cancellationToken);

    Task ReleaseAsync(ulong matchProposalId, bool cancelledByMerchant, string reason, DateTime now,
        CancellationToken cancellationToken);

    Task<int> PurgeChatAsync(ulong matchProposalId, CancellationToken cancellationToken);
}
