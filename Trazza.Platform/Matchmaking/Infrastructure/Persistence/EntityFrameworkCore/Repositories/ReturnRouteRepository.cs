using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     The POINT columns of return_routes are not mapped by EF Core. This repository writes them with
///     ST_PointFromText, reads them with ST_Latitude and ST_Longitude, and filters by distance in memory.
/// </summary>
public class ReturnRouteRepository(AppDbContext context) : BaseRepository<ReturnRoute>(context), IReturnRouteRepository
{
    public override async Task AddAsync(ReturnRoute route, CancellationToken cancellationToken = default)
    {
        // The insert and LAST_INSERT_ID() must run on the same connection, so they share a transaction
        // that the UnitOfWork commits.
        if (Context.Database.CurrentTransaction is null)
            await Context.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        route.CreatedAt = now;
        route.UpdatedAt = now;

        ulong carrierProfileId = route.CarrierProfileId;
        ulong vehicleId = route.VehicleId;
        ulong? driverId = route.DriverId?.Value;
        var origin = route.Origin;
        var destination = route.Destination;
        var departureAt = route.DepartureAt.UtcDateTime;
        var createdAt = now.UtcDateTime;
        var status = route.Status.ToSnakeCaseValue().ToUpperInvariant();

        await Context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO return_routes (carrier_profile_id, vehicle_id, driver_id, origin_address, origin_location,
                destination_address, destination_location, departure_at, max_shipments, status, created_at, updated_at)
            VALUES ({carrierProfileId}, {vehicleId}, {driverId},
                {origin.Address},
                ST_PointFromText(CONCAT('POINT(', CAST({origin.Location.Latitude} AS DECIMAL(11,8)), ' ', CAST({origin.Location.Longitude} AS DECIMAL(11,8)), ')'), 4326),
                {destination.Address},
                ST_PointFromText(CONCAT('POINT(', CAST({destination.Location.Latitude} AS DECIMAL(11,8)), ' ', CAST({destination.Location.Longitude} AS DECIMAL(11,8)), ')'), 4326),
                {departureAt}, {route.MaxShipments}, {status}, {createdAt}, {createdAt})",
            cancellationToken);

        var id = await Context.Database.SqlQueryRaw<ulong>("SELECT LAST_INSERT_ID() AS `Value`")
            .SingleAsync(cancellationToken);

        route.AssignGeneratedId(id);
        Context.Attach(route);
    }

    public override async Task<ReturnRoute?> FindByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
    {
        var route = await base.FindByIdAsync(id, cancellationToken);
        if (route is not null) await HydrateLocationsAsync([route], cancellationToken);
        return route;
    }

    public override async Task<ReturnRoute?> FindByIdAsNoTrackingAsync<TId>(TId id,
        CancellationToken cancellationToken = default)
    {
        var route = await base.FindByIdAsNoTrackingAsync(id, cancellationToken);
        if (route is not null) await HydrateLocationsAsync([route], cancellationToken);
        return route;
    }

    public override async Task<IEnumerable<ReturnRoute>> ListAsNoTrackingAsync(
        CancellationToken cancellationToken = default)
    {
        var routes = (await base.ListAsNoTrackingAsync(cancellationToken)).ToList();
        await HydrateLocationsAsync(routes, cancellationToken);
        return routes;
    }

    public async Task<IEnumerable<ReturnRoute>> FindByCarrierProfileIdAsNoTrackingAsync(
        CarrierProfileId carrierProfileId, CancellationToken cancellationToken)
    {
        var routes = await Context.Set<ReturnRoute>()
            .AsNoTracking()
            .Where(route => route.CarrierProfileId == carrierProfileId)
            .OrderByDescending(route => route.DepartureAt)
            .ToListAsync(cancellationToken);

        await HydrateLocationsAsync(routes, cancellationToken);
        return routes;
    }

    public async Task<ReturnRoute?> FindByIdForUpdateAsync(ReturnRouteId returnRouteId,
        CancellationToken cancellationToken)
    {
        // The row lock only lasts while a transaction is open. The UnitOfWork commits it after saving.
        if (Context.Database.CurrentTransaction is null)
            await Context.Database.BeginTransactionAsync(cancellationToken);

        ulong id = returnRouteId;
        var routes = await Context.Set<ReturnRoute>()
            .FromSqlInterpolated($"SELECT * FROM return_routes WHERE id = {id} FOR UPDATE")
            .ToListAsync(cancellationToken);

        var route = routes.FirstOrDefault();
        if (route is not null) await HydrateLocationsAsync([route], cancellationToken);
        return route;
    }

    public async Task<IEnumerable<ReturnRoute>> SearchPublishedAsNoTrackingAsync(GeoLocation pickup,
        GeoLocation delivery, TimeWindow pickupWindow, double maxDistanceKm, CancellationToken cancellationToken)
    {
        var from = pickupWindow.From;
        var until = pickupWindow.Until;

        var routes = await Context.Set<ReturnRoute>()
            .AsNoTracking()
            .Where(route => route.Status == EReturnRouteStatus.Published &&
                            route.DepartureAt >= from && route.DepartureAt <= until)
            .ToListAsync(cancellationToken);

        await HydrateLocationsAsync(routes, cancellationToken);

        return routes
            .Where(route => route.Origin.Location.DistanceKmTo(pickup) <= maxDistanceKm &&
                            route.Destination.Location.DistanceKmTo(delivery) <= maxDistanceKm)
            .OrderBy(route => route.Origin.Location.DistanceKmTo(pickup) +
                              route.Destination.Location.DistanceKmTo(delivery))
            .ToList();
    }

    private async Task HydrateLocationsAsync(IReadOnlyCollection<ReturnRoute> routes,
        CancellationToken cancellationToken)
    {
        if (routes.Count == 0) return;

        // The ids come from the database, so joining them in the text is safe.
        var ids = string.Join(",", routes.Select(route => route.Id.Value));
        var sql =
            "SELECT id AS Id, " +
            "ST_Latitude(origin_location) AS FirstLatitude, ST_Longitude(origin_location) AS FirstLongitude, " +
            "ST_Latitude(destination_location) AS SecondLatitude, " +
            "ST_Longitude(destination_location) AS SecondLongitude " +
            $"FROM return_routes WHERE id IN ({ids})";
        var rows = await Context.Database.SqlQueryRaw<LocationRow>(sql)
            .ToListAsync(cancellationToken);

        var locations = rows.ToDictionary(row => row.Id);
        foreach (var route in routes)
        {
            if (!locations.TryGetValue(route.Id.Value, out var row)) continue;
            route.ApplyLocations(new GeoLocation(row.FirstLatitude, row.FirstLongitude),
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
}
