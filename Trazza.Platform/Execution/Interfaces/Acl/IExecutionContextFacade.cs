namespace Trazza.Platform.Execution.Interfaces.Acl;

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

public interface IExecutionContextFacade
{
    Task<Guid> CreateTripAsync(ulong matchProposalId, ulong? driverId, decimal agreedRate, string currency,
        ulong acceptedByUserId, CancellationToken ct = default);

    Task<Dictionary<ulong, Guid>> FindTripPublicIdsByProposalIdsAsync(IReadOnlyCollection<ulong> matchProposalIds,
        CancellationToken ct = default);

    Task<bool> IsTripClosedAsync(ulong matchProposalId, CancellationToken ct = default);

    Task<TripSnapshot?> FindTripAsync(Guid tripPublicId, CancellationToken ct = default);

    Task<List<TripSnapshot>> ListTripsAsync(IReadOnlyCollection<ulong> tripIds, CancellationToken ct = default);

    Task<List<TripSnapshot>> ListTripsOfUserAsync(ulong userId, CancellationToken ct = default);

    Task<bool> LoanIotDevicesAsync(IReadOnlyCollection<ulong> vehicleIds, DateTime now, CancellationToken ct = default);

    Task ReturnIotDevicesAsync(IReadOnlyCollection<ulong> vehicleIds, CancellationToken ct = default);
}
