using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Model.Commands;
using Trazza.Platform.Execution.Domain.Repositories;
using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Execution.Application.Internal.CommandServices;

/// <summary>
///     GPS points sent by the carrier's phone (or by the simulator / an IoT device). A point farther than
///     <see cref="DeviationThresholdKm" /> from the expected path raises a deviation alert.
/// </summary>
public class TrackingCommandService(
    TripAccessService tripAccessService,
    ITrackingPointRepository trackingPointRepository,
    IDeviationAlertRepository deviationAlertRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ITrackingCommandService
{
    public const double DeviationThresholdKm = 2;
    private const decimal MaxSpeedKmh = 300;
    private static readonly TimeSpan AlertCooldown = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan MaxClockSkew = TimeSpan.FromMinutes(5);

    public Task<Result<TrackingResult, CommandError>> Handle(RecordTrackingPointCommand command,
        CancellationToken ct = default) =>
        unitOfWork.ExecuteInTransactionAsync(() => RecordAsync(command, ct), result => result.IsSuccess, ct);

    public async Task<Result<bool, CommandError>> Handle(AcknowledgeDeviationAlertCommand command,
        CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        var acknowledged = await deviationAlertRepository.AcknowledgeAsync(command.AlertId, access.Trip.Id,
            timeProvider.GetUtcNow().UtcDateTime, ct);
        if (!acknowledged) return CommandError.NotFound("alert-not-found");
        return true;
    }

    /// <summary>Expected path: route origin, pick-up, delivery and route destination.</summary>
    public static double DistanceToPathKm(GeoLocation point, IReadOnlyList<GeoLocation> path)
    {
        var best = double.MaxValue;
        for (var i = 0; i < path.Count - 1; i++) best = Math.Min(best, point.DistanceToSegment(path[i], path[i + 1]));
        return best;
    }

    private async Task<Result<TrackingResult, CommandError>> RecordAsync(RecordTrackingPointCommand command,
        CancellationToken ct)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        if (!access.IsCarrier) return CommandError.Forbidden("only-carrier");
        if (!access.Trip.IsOnTheRoad) return CommandError.Conflict("trip-not-on-the-road");

        var location = GeoLocation.Create(command.Latitude, command.Longitude);
        if (location.IsFailure) return location.Error!;
        if (command.SpeedKmh is < 0 or > MaxSpeedKmh) return CommandError.Validation("speed-invalid");

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var recordedAt = command.RecordedAt ?? now;
        if (recordedAt > now + MaxClockSkew) return CommandError.Validation("recorded-at-invalid");

        var last = await trackingPointRepository.FindLastAsync(access.Trip.Id, ct);
        var addedKm = last is null ? 0 : new GeoLocation(last.Latitude, last.Longitude).DistanceTo(location.Value!);

        var point = await trackingPointRepository.AddAsync(new TrackingPoint(0, access.Trip.Id, location.Value!.Latitude,
            location.Value.Longitude, command.SpeedKmh is null ? null : Math.Round(command.SpeedKmh.Value, 1),
            command.Source, recordedAt), ct);

        DeviationAlert? alert = null;
        var context = access.Context;
        var deviationKm = DistanceToPathKm(location.Value,
            [context.Origin, context.Pickup, context.Delivery, context.Destination]);
        if (deviationKm > DeviationThresholdKm)
        {
            var lastOpen = await deviationAlertRepository.FindLastOpenAsync(access.Trip.Id, ct);
            if (lastOpen is null || lastOpen.DetectedAt < now - AlertCooldown)
                alert = await deviationAlertRepository.AddAsync(new DeviationAlert(0, access.Trip.Id,
                    location.Value.Latitude, location.Value.Longitude, Math.Round((decimal)deviationKm, 2), now, now,
                    null), ct);
        }

        access.Trip.RegisterMovement(addedKm, command.UserId, now);
        await unitOfWork.CompleteAsync(ct);
        return new TrackingResult(point, alert, EnumText.ToText(access.Trip.Status));
    }
}
