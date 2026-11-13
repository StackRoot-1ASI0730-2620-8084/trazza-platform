using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Iam.Domain.Repositories;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Trazza.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Context.Set<User>().Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> FindByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default) =>
        Context.Set<User>().Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.PublicId == publicId, cancellationToken);

    public Task<User?> FindWithRolesAsync(ulong id, CancellationToken cancellationToken = default) =>
        Context.Set<User>().Include(user => user.Roles)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Context.Set<User>().AnyAsync(user => user.Email == email, cancellationToken);
}

public class PersonIdentityRepository(AppDbContext context)
    : BaseRepository<PersonIdentity>(context), IPersonIdentityRepository
{
    public Task<PersonIdentity?> FindByUserIdAsync(ulong userId, CancellationToken cancellationToken = default) =>
        Context.Set<PersonIdentity>().FirstOrDefaultAsync(identity => identity.UserId == userId, cancellationToken);

    public Task<bool> ExistsByDniHashAsync(byte[] dniHash, CancellationToken cancellationToken = default) =>
        Context.Set<PersonIdentity>().AnyAsync(identity => identity.DniHash == dniHash, cancellationToken);
}

public class IdentityVerificationRepository(AppDbContext context)
    : BaseRepository<IdentityVerification>(context), IIdentityVerificationRepository
{
    public Task<List<IdentityVerification>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default) =>
        Context.Set<IdentityVerification>().Where(verification => verification.UserId == userId)
            .OrderByDescending(verification => verification.SubmittedAt).ToListAsync(cancellationToken);
}

public class RefreshTokenRepository(AppDbContext context)
    : BaseRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByHashAsync(byte[] tokenHash, CancellationToken cancellationToken = default) =>
        Context.Set<RefreshToken>().FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
}

public class CarrierProfileRepository(AppDbContext context)
    : BaseRepository<CarrierProfile>(context), ICarrierProfileRepository
{
    public Task<CarrierProfile?> FindByUserIdAsync(ulong userId, CancellationToken cancellationToken = default) =>
        Context.Set<CarrierProfile>().FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
}

public class MerchantProfileRepository(AppDbContext context)
    : BaseRepository<MerchantProfile>(context), IMerchantProfileRepository
{
    public Task<MerchantProfile?> FindByUserIdAsync(ulong userId, CancellationToken cancellationToken = default) =>
        Context.Set<MerchantProfile>().FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);

    public Task<bool> ExistsByRucAsync(string ruc, CancellationToken cancellationToken = default) =>
        Context.Set<MerchantProfile>().AnyAsync(profile => profile.Ruc == ruc, cancellationToken);
}

public class DriverRepository(AppDbContext context) : BaseRepository<Driver>(context), IDriverRepository
{
    public Task<List<Driver>> ListByCarrierAsync(ulong carrierProfileId, CancellationToken cancellationToken = default) =>
        Context.Set<Driver>().Where(driver => driver.CarrierProfileId == carrierProfileId)
            .OrderBy(driver => driver.FullName).ToListAsync(cancellationToken);
}

public class VehicleRepository(AppDbContext context) : BaseRepository<Vehicle>(context), IVehicleRepository
{
    public Task<List<Vehicle>> ListByCarrierAsync(ulong carrierProfileId, CancellationToken cancellationToken = default) =>
        Context.Set<Vehicle>().Where(vehicle => vehicle.CarrierProfileId == carrierProfileId)
            .OrderBy(vehicle => vehicle.Plate).ToListAsync(cancellationToken);

    public Task<List<Vehicle>> ListByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken = default) =>
        Context.Set<Vehicle>().Where(vehicle => ids.Contains(vehicle.Id)).ToListAsync(cancellationToken);

    public Task<Vehicle?> FindByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default) =>
        Context.Set<Vehicle>().FirstOrDefaultAsync(vehicle => vehicle.PublicId == publicId, cancellationToken);

    public Task<bool> ExistsByPlateAsync(string plate, ulong? exceptVehicleId,
        CancellationToken cancellationToken = default) =>
        Context.Set<Vehicle>().AnyAsync(vehicle => vehicle.Plate == plate &&
                                                   (exceptVehicleId == null || vehicle.Id != exceptVehicleId),
            cancellationToken);
}

/// <summary>
///     SQL repository: <c>saved_addresses.location</c> is <c>POINT SRID 4326</c>, written with
///     <c>ST_PointFromText</c> and read with <c>ST_Latitude</c>/<c>ST_Longitude</c>.
/// </summary>
public class SavedAddressRepository(AppDbContext context) : ISavedAddressRepository
{
    public async Task<SavedAddress> AddAsync(SavedAddress address, CancellationToken cancellationToken = default)
    {
        var wkt = new GeoLocation(address.Latitude, address.Longitude).ToWkt();
        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO saved_addresses (user_id, label, address, district, location, created_at)
                VALUES ({address.UserId}, {address.Label}, {address.Address}, {address.District},
                        ST_PointFromText({wkt}, 4326, 'axis-order=lat-long'), {address.CreatedAt})", cancellationToken);
            var ids = await context.Database.SqlQueryRaw<ulong>("SELECT LAST_INSERT_ID() AS Value")
                .ToListAsync(cancellationToken);
            return address with { Id = ids[0] };
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    public async Task<List<SavedAddress>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default)
    {
        var rows = await context.Database.SqlQuery<SavedAddressRow>($@"
                SELECT id AS Id, user_id AS UserId, label AS Label, address AS Address, district AS District,
                       ST_Latitude(location) AS Latitude, ST_Longitude(location) AS Longitude, created_at AS CreatedAt
                FROM saved_addresses WHERE user_id = {userId} ORDER BY label")
            .ToListAsync(cancellationToken);
        return rows.Select(row => new SavedAddress(row.Id, row.UserId, row.Label, row.Address, row.District,
            row.Latitude, row.Longitude, row.CreatedAt)).ToList();
    }

    public async Task<bool> DeleteAsync(ulong id, ulong userId, CancellationToken cancellationToken = default)
    {
        var affected = await context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM saved_addresses WHERE id = {id} AND user_id = {userId}", cancellationToken);
        return affected > 0;
    }

    public sealed class SavedAddressRow
    {
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? District { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
