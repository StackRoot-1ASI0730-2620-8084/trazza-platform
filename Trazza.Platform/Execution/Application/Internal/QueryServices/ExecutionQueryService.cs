using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Repositories;
using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Matchmaking.Interfaces.Acl;
using Trazza.Platform.Shared.Domain.Model;

namespace Trazza.Platform.Execution.Application.Internal.QueryServices;

public class ExecutionQueryService(
    TripAccessService tripAccessService,
    ITripRepository tripRepository,
    ITrackingPointRepository trackingPointRepository,
    IDeviationAlertRepository deviationAlertRepository,
    IIncidentRepository incidentRepository,
    IIotDeviceRepository iotDeviceRepository,
    IMatchmakingContextFacade matchmakingContextFacade,
    IIamContextFacade iamContextFacade) : IExecutionQueryService
{
    public async Task<List<TripView>> ListMyTripsAsync(ulong userId, CancellationToken ct = default)
    {
        var carrierProfileId = await iamContextFacade.FindCarrierProfileIdAsync(userId, ct);
        var merchantProfileId = await iamContextFacade.FindMerchantProfileIdAsync(userId, ct);
        var proposalIds = await matchmakingContextFacade.ListProposalIdsWithTripsAsync(carrierProfileId, merchantProfileId, ct);
        var trips = await tripRepository.ListByProposalIdsAsync(proposalIds, ct);

        var views = new List<TripView>();
        foreach (var access in await tripAccessService.BuildAccessAsync(userId, trips, ct))
            views.Add(await tripAccessService.BuildViewAsync(access, ct));
        return views.OrderByDescending(view => view.Trip.CreatedAt).ToList();
    }

    public async Task<Result<TripView, CommandError>> GetTripAsync(ulong userId, Guid tripId, CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(userId, tripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        return await tripAccessService.BuildViewAsync(access, ct);
    }

    public async Task<Result<List<TripStatusChange>, CommandError>> ListHistoryAsync(ulong userId, Guid tripId,
        CancellationToken ct = default)
    {
        if (await tripAccessService.FindAsync(userId, tripId, ct) is null) return CommandError.NotFound("trip-not-found");
        var trip = await tripRepository.FindByPublicIdAsync(tripId, true, ct);
        return trip!.History.OrderBy(change => change.ChangedAt).ToList();
    }

    public async Task<Result<List<TrackingPoint>, CommandError>> ListTrackingPointsAsync(ulong userId, Guid tripId,
        CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(userId, tripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        return await trackingPointRepository.ListByTripAsync(access.Trip.Id, ct);
    }

    public async Task<Result<List<DeviationAlert>, CommandError>> ListDeviationAlertsAsync(ulong userId, Guid tripId,
        CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(userId, tripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        return await deviationAlertRepository.ListByTripAsync(access.Trip.Id, ct);
    }

    public async Task<Result<List<Incident>, CommandError>> ListIncidentsAsync(ulong userId, Guid tripId,
        CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(userId, tripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        return await incidentRepository.ListByTripAsync(access.Trip.Id, ct);
    }

    public async Task<List<IotDeviceView>> ListMyIotDevicesAsync(ulong userId, CancellationToken ct = default)
    {
        var carrierProfileId = await iamContextFacade.FindCarrierProfileIdAsync(userId, ct);
        if (carrierProfileId is null) return [];
        // Vehicles of the carrier are read through IAM; the facade only exposes them by id, so list them by profile.
        var vehicles = await iamContextFacade.ListVehiclesOfCarrierAsync(carrierProfileId.Value, ct);
        var devices = await iotDeviceRepository.ListByVehicleIdsAsync(vehicles.Select(vehicle => vehicle.Id).ToList(), ct);
        return devices.Select(device =>
        {
            var vehicle = vehicles.First(item => item.Id == device.AssignedVehicleId);
            return new IotDeviceView(device, vehicle.PublicId, vehicle.Plate);
        }).ToList();
    }
}
