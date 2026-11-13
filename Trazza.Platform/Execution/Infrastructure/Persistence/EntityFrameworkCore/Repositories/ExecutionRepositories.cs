using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Model.ValueObjects;
using Trazza.Platform.Execution.Domain.Repositories;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Spatial;

namespace Trazza.Platform.Execution.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class TripRepository(AppDbContext context) : BaseRepository<Trip>(context), ITripRepository
{
    public async Task<Trip?> FindByPublicIdAsync(Guid publicId, bool includeHistory = false,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Set<Trip>().AsQueryable();
        if (includeHistory) query = query.Include(trip => trip.History);
        return await query.FirstOrDefaultAsync(trip => trip.PublicId == publicId, cancellationToken);
    }

    public async Task<Trip?> FindByProposalIdAsync(ulong matchProposalId, CancellationToken cancellationToken = default) =>
        await Context.Set<Trip>().FirstOrDefaultAsync(trip => trip.MatchProposalId == matchProposalId, cancellationToken);

    public async Task<List<Trip>> ListByProposalIdsAsync(IReadOnlyCollection<ulong> matchProposalIds,
        CancellationToken cancellationToken = default)
    {
        if (matchProposalIds.Count == 0) return [];
        return await Context.Set<Trip>()
            .Where(trip => matchProposalIds.Contains(trip.MatchProposalId))
            .ToListAsync(cancellationToken);
    }
}

public class TrackingPointRepository(AppDbContext context) : ITrackingPointRepository
{
    public async Task<TrackingPoint> AddAsync(TrackingPoint point, CancellationToken cancellationToken = default)
    {
        var wkt = new GeoLocation(point.Latitude, point.Longitude).ToWkt();
        var source = EnumText.ToText(point.Source);
        var id = await context.Database.InsertAndGetIdAsync($@"
            INSERT INTO tracking_points (trip_id, location, speed_kmh, source, recorded_at)
            VALUES ({point.TripId}, ST_PointFromText({wkt}, 4326, 'axis-order=lat-long'), {point.SpeedKmh}, {source},
                    {point.RecordedAt})", cancellationToken);
        return point with { Id = id };
    }

    public async Task<List<TrackingPoint>> ListByTripAsync(ulong tripId, CancellationToken cancellationToken = default) =>
        (await context.Database.SqlQuery<TrackingPointRow>($@"
                SELECT id AS Id, trip_id AS TripId, ST_Latitude(location) AS Latitude, ST_Longitude(location) AS Longitude,
                       speed_kmh AS SpeedKmh, source AS Source, recorded_at AS RecordedAt
                FROM tracking_points WHERE trip_id = {tripId} ORDER BY recorded_at, id")
            .ToListAsync(cancellationToken))
        .Select(row => row.ToPoint())
        .ToList();

    public async Task<TrackingPoint?> FindLastAsync(ulong tripId, CancellationToken cancellationToken = default) =>
        (await context.Database.SqlQuery<TrackingPointRow>($@"
                SELECT id AS Id, trip_id AS TripId, ST_Latitude(location) AS Latitude, ST_Longitude(location) AS Longitude,
                       speed_kmh AS SpeedKmh, source AS Source, recorded_at AS RecordedAt
                FROM tracking_points WHERE trip_id = {tripId} ORDER BY recorded_at DESC, id DESC LIMIT 1")
            .ToListAsync(cancellationToken))
        .Select(row => row.ToPoint())
        .FirstOrDefault();

    public sealed class TrackingPointRow
    {
        public ulong Id { get; set; }
        public ulong TripId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal? SpeedKmh { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime RecordedAt { get; set; }

        public TrackingPoint ToPoint() =>
            new(Id, TripId, Latitude, Longitude, SpeedKmh, EnumText.FromText<TrackingSource>(Source),
                DateTime.SpecifyKind(RecordedAt, DateTimeKind.Utc));
    }
}

public class DeviationAlertRepository(AppDbContext context) : IDeviationAlertRepository
{
    public async Task<DeviationAlert> AddAsync(DeviationAlert alert, CancellationToken cancellationToken = default)
    {
        var wkt = new GeoLocation(alert.Latitude, alert.Longitude).ToWkt();
        var id = await context.Database.InsertAndGetIdAsync($@"
            INSERT INTO deviation_alerts (trip_id, location, deviation_km, detected_at, notified_at, acknowledged_at)
            VALUES ({alert.TripId}, ST_PointFromText({wkt}, 4326, 'axis-order=lat-long'), {alert.DeviationKm},
                    {alert.DetectedAt}, {alert.NotifiedAt}, {alert.AcknowledgedAt})", cancellationToken);
        return alert with { Id = id };
    }

    public async Task<List<DeviationAlert>> ListByTripAsync(ulong tripId, CancellationToken cancellationToken = default) =>
        (await context.Database.SqlQuery<DeviationAlertRow>($@"
                SELECT id AS Id, trip_id AS TripId, ST_Latitude(location) AS Latitude, ST_Longitude(location) AS Longitude,
                       deviation_km AS DeviationKm, detected_at AS DetectedAt, notified_at AS NotifiedAt,
                       acknowledged_at AS AcknowledgedAt
                FROM deviation_alerts WHERE trip_id = {tripId} ORDER BY detected_at DESC")
            .ToListAsync(cancellationToken))
        .Select(row => row.ToAlert())
        .ToList();

    public async Task<DeviationAlert?> FindLastOpenAsync(ulong tripId, CancellationToken cancellationToken = default) =>
        (await context.Database.SqlQuery<DeviationAlertRow>($@"
                SELECT id AS Id, trip_id AS TripId, ST_Latitude(location) AS Latitude, ST_Longitude(location) AS Longitude,
                       deviation_km AS DeviationKm, detected_at AS DetectedAt, notified_at AS NotifiedAt,
                       acknowledged_at AS AcknowledgedAt
                FROM deviation_alerts WHERE trip_id = {tripId} AND acknowledged_at IS NULL
                ORDER BY detected_at DESC LIMIT 1")
            .ToListAsync(cancellationToken))
        .Select(row => row.ToAlert())
        .FirstOrDefault();

    public async Task<bool> AcknowledgeAsync(ulong alertId, ulong tripId, DateTime now,
        CancellationToken cancellationToken = default) =>
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE deviation_alerts SET acknowledged_at = COALESCE(acknowledged_at, {now})
            WHERE id = {alertId} AND trip_id = {tripId}", cancellationToken) > 0;

    public sealed class DeviationAlertRow
    {
        public ulong Id { get; set; }
        public ulong TripId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal DeviationKm { get; set; }
        public DateTime DetectedAt { get; set; }
        public DateTime? NotifiedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }

        public DeviationAlert ToAlert() =>
            new(Id, TripId, Latitude, Longitude, DeviationKm, DateTime.SpecifyKind(DetectedAt, DateTimeKind.Utc),
                NotifiedAt.HasValue ? DateTime.SpecifyKind(NotifiedAt.Value, DateTimeKind.Utc) : null,
                AcknowledgedAt.HasValue ? DateTime.SpecifyKind(AcknowledgedAt.Value, DateTimeKind.Utc) : null);
    }
}

public class IncidentRepository(AppDbContext context) : BaseRepository<Incident>(context), IIncidentRepository
{
    public async Task<List<Incident>> ListByTripAsync(ulong tripId, CancellationToken cancellationToken = default) =>
        await Context.Set<Incident>()
            .Include(incident => incident.Evidences)
            .Where(incident => incident.TripId == tripId)
            .OrderByDescending(incident => incident.ReportedAt)
            .ToListAsync(cancellationToken);

    public async Task<Incident?> FindWithEvidencesAsync(ulong incidentId, ulong tripId,
        CancellationToken cancellationToken = default) =>
        await Context.Set<Incident>()
            .Include(incident => incident.Evidences)
            .FirstOrDefaultAsync(incident => incident.Id == incidentId && incident.TripId == tripId, cancellationToken);
}

public class IotDeviceRepository(AppDbContext context) : BaseRepository<IotDevice>(context), IIotDeviceRepository
{
    public async Task<List<IotDevice>> ListAvailableAsync(int count, CancellationToken cancellationToken = default) =>
        await Context.Set<IotDevice>()
            .Where(device => device.Status == IotDeviceStatus.Available && device.AssignedVehicleId == null)
            .OrderBy(device => device.Id)
            .Take(count)
            .ToListAsync(cancellationToken);

    public async Task<List<IotDevice>> ListByVehicleIdsAsync(IReadOnlyCollection<ulong> vehicleIds,
        CancellationToken cancellationToken = default)
    {
        if (vehicleIds.Count == 0) return [];
        return await Context.Set<IotDevice>()
            .Where(device => device.AssignedVehicleId.HasValue && vehicleIds.Contains(device.AssignedVehicleId.Value))
            .ToListAsync(cancellationToken);
    }
}
