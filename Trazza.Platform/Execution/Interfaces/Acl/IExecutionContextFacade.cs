namespace Trazza.Platform.Execution.Interfaces.Acl;

/// <summary>Trip as seen by Billing and Reputation.</summary>
public record TripSnapshot(
    ulong Id,
    Guid PublicId,
    string Status,
    decimal AgreedRate,
    string Currency,
    ulong CarrierUserId,
    ulong MerchantUserId,
    DateTime? DeliveredAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    string OriginAddress,
    string DestinationAddress);

/// <summary>
///     Anti-corruption layer used by Matchmaking, Billing and Reputation to open and read trips and to lend IoT devices.
///     It depends only on Execution repositories and on the IAM and Matchmaking facades, which never call back.
/// </summary>
public interface IExecutionContextFacade
{
    /// <summary>Opens and saves the trip of an accepted proposal (status SCHEDULED) and returns its public id.</summary>
    Task<Guid> CreateTripAsync(ulong matchProposalId, ulong? driverId, decimal agreedRate, string currency,
        ulong acceptedByUserId, CancellationToken ct = default);

    Task<Dictionary<ulong, Guid>> FindTripPublicIdsByProposalIdsAsync(IReadOnlyCollection<ulong> matchProposalIds,
        CancellationToken ct = default);

    /// <summary>True when the trip of the proposal was confirmed or cancelled (its chat is closed).</summary>
    Task<bool> IsTripClosedAsync(ulong matchProposalId, CancellationToken ct = default);

    Task<TripSnapshot?> FindTripAsync(Guid tripPublicId, CancellationToken ct = default);

    Task<List<TripSnapshot>> ListTripsAsync(IReadOnlyCollection<ulong> tripIds, CancellationToken ct = default);

    /// <summary>Trips where the user is the carrier or the merchant.</summary>
    Task<List<TripSnapshot>> ListTripsOfUserAsync(ulong userId, CancellationToken ct = default);

    /// <summary>Lends one available IoT device to each vehicle. False when there are not enough devices.</summary>
    Task<bool> LoanIotDevicesAsync(IReadOnlyCollection<ulong> vehicleIds, DateTime now, CancellationToken ct = default);

    Task ReturnIotDevicesAsync(IReadOnlyCollection<ulong> vehicleIds, CancellationToken ct = default);
}
