using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Repositories;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Matchmaking.Interfaces.Acl;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Execution.Interfaces.Acl;

public class ExecutionContextFacade(
    ITripRepository tripRepository,
    IIotDeviceRepository iotDeviceRepository,
    IMatchmakingContextFacade matchmakingContextFacade,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IExecutionContextFacade
{
    public async Task<Guid> CreateTripAsync(ulong matchProposalId, ulong? driverId, decimal agreedRate, string currency,
        ulong acceptedByUserId, CancellationToken ct = default)
    {
        var trip = new Trip(matchProposalId, driverId, agreedRate, currency, acceptedByUserId,
            timeProvider.GetUtcNow().UtcDateTime);
        await tripRepository.AddAsync(trip, ct);
        await unitOfWork.CompleteAsync(ct);
        return trip.PublicId;
    }

    public async Task<Dictionary<ulong, Guid>> FindTripPublicIdsByProposalIdsAsync(
        IReadOnlyCollection<ulong> matchProposalIds, CancellationToken ct = default) =>
        (await tripRepository.ListByProposalIdsAsync(matchProposalIds, ct))
        .ToDictionary(trip => trip.MatchProposalId, trip => trip.PublicId);

    public async Task<bool> IsTripClosedAsync(ulong matchProposalId, CancellationToken ct = default) =>
        (await tripRepository.FindByProposalIdAsync(matchProposalId, ct))?.IsClosed ?? false;

    public async Task<TripSnapshot?> FindTripAsync(Guid tripPublicId, CancellationToken ct = default)
    {
        var trip = await tripRepository.FindByPublicIdAsync(tripPublicId, false, ct);
        return trip is null ? null : (await ToSnapshotsAsync([trip], ct)).FirstOrDefault();
    }

    public async Task<List<TripSnapshot>> ListTripsAsync(IReadOnlyCollection<ulong> tripIds, CancellationToken ct = default)
    {
        var trips = new List<Trip>();
        foreach (var id in tripIds.Distinct())
        {
            var trip = await tripRepository.FindByIdAsync(id, ct);
            if (trip is not null) trips.Add(trip);
        }

        return await ToSnapshotsAsync(trips, ct);
    }

    public async Task<List<TripSnapshot>> ListTripsOfUserAsync(ulong userId, CancellationToken ct = default)
    {
        var carrierProfileId = await iamContextFacade.FindCarrierProfileIdAsync(userId, ct);
        var merchantProfileId = await iamContextFacade.FindMerchantProfileIdAsync(userId, ct);
        var proposalIds = await matchmakingContextFacade.ListProposalIdsWithTripsAsync(carrierProfileId, merchantProfileId, ct);
        return await ToSnapshotsAsync(await tripRepository.ListByProposalIdsAsync(proposalIds, ct), ct);
    }

    public async Task<bool> LoanIotDevicesAsync(IReadOnlyCollection<ulong> vehicleIds, DateTime now,
        CancellationToken ct = default)
    {
        var alreadyLoaned = (await iotDeviceRepository.ListByVehicleIdsAsync(vehicleIds, ct))
            .Select(device => device.AssignedVehicleId!.Value)
            .ToHashSet();
        var missing = vehicleIds.Distinct().Where(id => !alreadyLoaned.Contains(id)).ToList();
        if (missing.Count == 0) return true;

        var available = await iotDeviceRepository.ListAvailableAsync(missing.Count, ct);
        if (available.Count < missing.Count) return false;
        for (var i = 0; i < missing.Count; i++) available[i].LoanTo(missing[i], now);
        return true;
    }

    public async Task ReturnIotDevicesAsync(IReadOnlyCollection<ulong> vehicleIds, CancellationToken ct = default)
    {
        foreach (var device in await iotDeviceRepository.ListByVehicleIdsAsync(vehicleIds, ct)) device.Return();
    }

    private async Task<List<TripSnapshot>> ToSnapshotsAsync(IReadOnlyCollection<Trip> trips, CancellationToken ct)
    {
        if (trips.Count == 0) return [];
        var contexts = (await matchmakingContextFacade.ListTripContextsAsync(
                trips.Select(trip => trip.MatchProposalId).ToList(), ct))
            .ToDictionary(context => context.MatchProposalId);

        var snapshots = new List<TripSnapshot>();
        foreach (var trip in trips)
        {
            if (!contexts.TryGetValue(trip.MatchProposalId, out var context)) continue;
            var carrierUserId = await iamContextFacade.FindUserIdByCarrierProfileIdAsync(context.CarrierProfileId, ct) ?? 0;
            var merchantUserId = await iamContextFacade.FindUserIdByMerchantProfileIdAsync(context.MerchantProfileId, ct) ?? 0;
            snapshots.Add(new TripSnapshot(trip.Id, trip.PublicId, EnumText.ToText(trip.Status), trip.AgreedRate,
                trip.Currency, carrierUserId, merchantUserId, trip.DeliveredAt, trip.ConfirmedAt, trip.CancelledAt,
                context.OriginAddress, context.DestinationAddress));
        }

        return snapshots;
    }
}
