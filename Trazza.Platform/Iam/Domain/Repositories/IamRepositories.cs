using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Iam.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<User?> FindWithRolesAsync(ulong id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}

public interface IPersonIdentityRepository : IBaseRepository<PersonIdentity>
{
    Task<PersonIdentity?> FindByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByDniHashAsync(byte[] dniHash, CancellationToken cancellationToken = default);
}

public interface IIdentityVerificationRepository : IBaseRepository<IdentityVerification>
{
    Task<List<IdentityVerification>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default);
}

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> FindByHashAsync(byte[] tokenHash, CancellationToken cancellationToken = default);
}

public interface ICarrierProfileRepository : IBaseRepository<CarrierProfile>
{
    Task<CarrierProfile?> FindByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);
}

public interface IMerchantProfileRepository : IBaseRepository<MerchantProfile>
{
    Task<MerchantProfile?> FindByUserIdAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByRucAsync(string ruc, CancellationToken cancellationToken = default);
}

public interface IDriverRepository : IBaseRepository<Driver>
{
    Task<List<Driver>> ListByCarrierAsync(ulong carrierProfileId, CancellationToken cancellationToken = default);
}

public interface IVehicleRepository : IBaseRepository<Vehicle>
{
    Task<List<Vehicle>> ListByCarrierAsync(ulong carrierProfileId, CancellationToken cancellationToken = default);

    Task<List<Vehicle>> ListByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken = default);

    Task<Vehicle?> FindByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByPlateAsync(string plate, ulong? exceptVehicleId, CancellationToken cancellationToken = default);
}

/// <summary>
///     Saved addresses have a spatial column, so this repository writes immediately with SQL.
/// </summary>
public interface ISavedAddressRepository
{
    Task<SavedAddress> AddAsync(SavedAddress address, CancellationToken cancellationToken = default);

    Task<List<SavedAddress>> ListByUserAsync(ulong userId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(ulong id, ulong userId, CancellationToken cancellationToken = default);
}
