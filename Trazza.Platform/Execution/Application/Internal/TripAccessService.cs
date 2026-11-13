using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Repositories;
using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Matchmaking.Interfaces.Acl;

namespace Trazza.Platform.Execution.Application.Internal;

/// <summary>A trip with its route/shipment context and who takes part in it.</summary>
public record TripAccess(Trip Trip, TripContextSnapshot Context, ulong CarrierUserId, ulong MerchantUserId, ulong UserId)
{
    public bool IsCarrier => UserId == CarrierUserId;
    public bool IsMerchant => UserId == MerchantUserId;
}

/// <summary>
///     Loads trips only for their carrier or merchant, and builds <see cref="TripView" />.
/// </summary>
public class TripAccessService(
    ITripRepository tripRepository,
    ITrackingPointRepository trackingPointRepository,
    IDeviationAlertRepository deviationAlertRepository,
    IMatchmakingContextFacade matchmakingContextFacade,
    IIamContextFacade iamContextFacade)
{
    /// <summary>Null when the trip does not exist or the user is not part of it.</summary>
    public async Task<TripAccess?> FindAsync(ulong userId, Guid tripId, CancellationToken ct = default)
    {
        var trip = await tripRepository.FindByPublicIdAsync(tripId, false, ct);
        if (trip is null) return null;
        var access = (await BuildAccessAsync(userId, [trip], ct)).FirstOrDefault();
        return access is not null && (access.IsCarrier || access.IsMerchant) ? access : null;
    }

    public async Task<List<TripAccess>> BuildAccessAsync(ulong userId, IReadOnlyCollection<Trip> trips,
        CancellationToken ct = default)
    {
        var contexts = (await matchmakingContextFacade.ListTripContextsAsync(
                trips.Select(trip => trip.MatchProposalId).ToList(), ct))
            .ToDictionary(context => context.MatchProposalId);
        var result = new List<TripAccess>();
        foreach (var trip in trips)
        {
            if (!contexts.TryGetValue(trip.MatchProposalId, out var context)) continue;
            var carrierUserId = await iamContextFacade.FindUserIdByCarrierProfileIdAsync(context.CarrierProfileId, ct) ?? 0;
            var merchantUserId = await iamContextFacade.FindUserIdByMerchantProfileIdAsync(context.MerchantProfileId, ct) ?? 0;
            result.Add(new TripAccess(trip, context, carrierUserId, merchantUserId, userId));
        }

        return result;
    }

    public async Task<TripView> BuildViewAsync(TripAccess access, CancellationToken ct = default)
    {
        var carrier = await iamContextFacade.FindContactAsync(access.CarrierUserId, ct);
        var merchant = await iamContextFacade.FindContactAsync(access.MerchantUserId, ct);
        var vehicle = (await iamContextFacade.ListVehiclesAsync([access.Context.VehicleId], ct)).FirstOrDefault();
        var driverId = access.Trip.DriverId ?? access.Context.RouteDriverId;
        var driver = driverId.HasValue ? await iamContextFacade.FindDriverAsync(driverId.Value, ct) : null;
        var lastPoint = await trackingPointRepository.FindLastAsync(access.Trip.Id, ct);
        var openAlerts = (await deviationAlertRepository.ListByTripAsync(access.Trip.Id, ct))
            .Count(alert => alert.AcknowledgedAt is null);

        return new TripView(access.Trip, access.Context, access.CarrierUserId, carrier?.FullName ?? string.Empty,
            carrier?.Phone, access.MerchantUserId, merchant?.BusinessName ?? merchant?.FullName ?? string.Empty,
            merchant?.Phone, vehicle?.PublicId ?? Guid.Empty, vehicle?.Plate ?? string.Empty, driver?.FullName, lastPoint,
            openAlerts, access.IsCarrier);
    }
}
