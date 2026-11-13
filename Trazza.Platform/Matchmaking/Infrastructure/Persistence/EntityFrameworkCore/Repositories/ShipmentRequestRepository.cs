using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     The POINT columns of shipment_requests are not mapped by EF Core. This repository writes them with
///     ST_PointFromText, reads them with ST_Latitude and ST_Longitude, and filters by distance in memory.
/// </summary>
public class ShipmentRequestRepository(AppDbContext context)
    : BaseRepository<ShipmentRequest>(context), IShipmentRequestRepository
{
    public override async Task AddAsync(ShipmentRequest request, CancellationToken cancellationToken = default)
    {
        // The insert and LAST_INSERT_ID() must run on the same connection, so they share a transaction
        // that the UnitOfWork commits.
        if (Context.Database.CurrentTransaction is null)
            await Context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        request.CreatedAt = now;
        request.UpdatedAt = now;

        ulong merchantProfileId = request.MerchantProfileId;
        var publicId = request.PublicId.ToByteArray();
        var pickup = request.Pickup;
        var delivery = request.Delivery;
        var pickupFrom = request.PickupWindow.From.UtcDateTime;
        var pickupUntil = request.PickupWindow.Until.UtcDateTime;
        var createdAt = now.UtcDateTime;
        var status = request.Status.ToSnakeCaseValue().ToUpperInvariant();

        await Context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO shipment_requests (public_id, merchant_profile_id, description, weight_kg, volume_m3, pickup_address,
                pickup_location, delivery_address, delivery_location, pickup_from, pickup_until, status,
                created_at, updated_at)
            VALUES ({publicId}, {merchantProfileId}, {request.Description}, {request.Cargo.WeightKg}, {request.Cargo.VolumeM3},
                {pickup.Address},
                ST_PointFromText(CONCAT('POINT(', CAST({pickup.Location.Latitude} AS DECIMAL(11,8)), ' ', CAST({pickup.Location.Longitude} AS DECIMAL(11,8)), ')'), 4326),
                {delivery.Address},
                ST_PointFromText(CONCAT('POINT(', CAST({delivery.Location.Latitude} AS DECIMAL(11,8)), ' ', CAST({delivery.Location.Longitude} AS DECIMAL(11,8)), ')'), 4326),
                {pickupFrom}, {pickupUntil}, {status}, {createdAt}, {createdAt})",
            cancellationToken);

        var id = await Context.Database.SqlQueryRaw<ulong>("SELECT LAST_INSERT_ID() AS `Value`")
            .SingleAsync(cancellationToken);

        request.AssignGeneratedId(id);
        Context.Attach(request);
    }

    public override async Task<ShipmentRequest?> FindByIdAsync<TId>(TId id,
        CancellationToken cancellationToken = default)
    {
        var request = await base.FindByIdAsync(id, cancellationToken);
        if (request is not null) await HydrateLocationsAsync([request], cancellationToken);
        return request;
    }

    public override async Task<ShipmentRequest?> FindByIdAsNoTrackingAsync<TId>(TId id,
        CancellationToken cancellationToken = default)
    {
        var request = await base.FindByIdAsNoTrackingAsync(id, cancellationToken);
        if (request is not null) await HydrateLocationsAsync([request], cancellationToken);
        return request;
    }

    public override async Task<IEnumerable<ShipmentRequest>> ListAsNoTrackingAsync(
        CancellationToken cancellationToken = default)
    {
        var requests = (await base.ListAsNoTrackingAsync(cancellationToken)).ToList();
        await HydrateLocationsAsync(requests, cancellationToken);
        return requests;
    }

    public async Task<IEnumerable<ShipmentRequest>> FindByMerchantProfileIdAsNoTrackingAsync(
        MerchantProfileId merchantProfileId, CancellationToken cancellationToken)
    {
        var requests = await Context.Set<ShipmentRequest>()
            .AsNoTracking()
            .Where(request => request.MerchantProfileId == merchantProfileId)
            .OrderByDescending(request => request.PickupWindow.From)
            .ToListAsync(cancellationToken);

        await HydrateLocationsAsync(requests, cancellationToken);
        return requests;
    }

    public async Task<IEnumerable<ShipmentRequest>> SearchOpenAsNoTrackingAsync(GeoLocation origin,
        GeoLocation destination, DateTimeOffset departureAt, double maxDistanceKm,
        CancellationToken cancellationToken)
    {
        var requests = await Context.Set<ShipmentRequest>()
            .AsNoTracking()
            .Where(request => request.Status == EShipmentRequestStatus.Open &&
                              request.PickupWindow.From <= departureAt && request.PickupWindow.Until >= departureAt)
            .ToListAsync(cancellationToken);

        await HydrateLocationsAsync(requests, cancellationToken);

        return requests
            .Where(request => request.Pickup.Location.DistanceKmTo(origin) <= maxDistanceKm &&
                              request.Delivery.Location.DistanceKmTo(destination) <= maxDistanceKm)
            .OrderBy(request => request.Pickup.Location.DistanceKmTo(origin) +
                                request.Delivery.Location.DistanceKmTo(destination))
            .ToList();
    }

    private async Task HydrateLocationsAsync(IReadOnlyCollection<ShipmentRequest> requests,
        CancellationToken cancellationToken)
    {
        if (requests.Count == 0) return;

        // The ids come from the database, so joining them in the text is safe.
        var ids = string.Join(",", requests.Select(request => request.Id.Value));
        var sql =
            "SELECT id AS Id, " +
            "ST_Latitude(pickup_location) AS FirstLatitude, ST_Longitude(pickup_location) AS FirstLongitude, " +
            "ST_Latitude(delivery_location) AS SecondLatitude, " +
            "ST_Longitude(delivery_location) AS SecondLongitude " +
            $"FROM shipment_requests WHERE id IN ({ids})";
        var rows = await Context.Database.SqlQueryRaw<LocationRow>(sql)
            .ToListAsync(cancellationToken);

        var locations = rows.ToDictionary(row => row.Id);
        foreach (var request in requests)
        {
            if (!locations.TryGetValue(request.Id.Value, out var row)) continue;
            request.ApplyLocations(new GeoLocation(row.FirstLatitude, row.FirstLongitude),
                new GeoLocation(row.SecondLatitude, row.SecondLongitude));
        }
    }

    private sealed class LocationRow
    {
        public ulong Id { get; set; }
        public double FirstLatitude { get; set; }
        public double FirstLongitude { get; set; }
        public double SecondLatitude { get; set; }
        public double SecondLongitude { get; set; }
    }

    public async Task<ShipmentRequestId?> FindIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var ids = await Context.Set<ShipmentRequest>()
            .AsNoTracking()
            .Where(request => request.PublicId == publicId)
            .Select(request => request.Id)
            .Take(1)
            .ToListAsync(cancellationToken);
        return ids.Count == 0 ? null : ids[0];
    }

    public async Task<IReadOnlyDictionary<ulong, Guid>> FindPublicIdsAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return new Dictionary<ulong, Guid>();
        var keys = ids.Distinct().Select(id => new ShipmentRequestId(id)).ToList();
        var rows = await Context.Set<ShipmentRequest>()
            .AsNoTracking()
            .Where(request => keys.Contains(request.Id))
            .Select(request => new { request.Id, request.PublicId })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(row => row.Id.Value, row => row.PublicId);
    }
}
