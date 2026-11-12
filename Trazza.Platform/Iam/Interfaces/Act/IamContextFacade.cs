using TrazzaPlatform.API.Iam.Application.Internal.OutboundServices;
using TrazzaPlatform.API.Iam.Domain.Repositories;

namespace TrazzaPlatform.API.Iam.Interfaces.Acl;

public record VehicleSnapshot(
    ulong Id,
    Guid PublicId,
    ulong CarrierProfileId,
    string Plate,
    string Model,
    decimal MaxLoadKg,
    decimal CargoVolumeM3,
    bool IsActive);

public record DriverSnapshot(ulong Id, ulong CarrierProfileId, string FullName, byte ExperienceStars, bool IsActive);

public record ContactSnapshot(ulong UserId, Guid PublicId, string FullName, string? Phone, string? BusinessName);

/// <summary>
///     Anti-corruption layer used by the other bounded contexts to read IAM &amp; Profiles data.
/// </summary>
public interface IIamContextFacade
{
    Task<ulong?> FindCarrierProfileIdAsync(ulong userId, CancellationToken ct = default);

    Task<ulong?> FindMerchantProfileIdAsync(ulong userId, CancellationToken ct = default);

    Task<ulong?> FindUserIdByCarrierProfileIdAsync(ulong carrierProfileId, CancellationToken ct = default);

    Task<ulong?> FindUserIdByMerchantProfileIdAsync(ulong merchantProfileId, CancellationToken ct = default);

    Task<VehicleSnapshot?> FindVehicleAsync(Guid publicId, CancellationToken ct = default);

    Task<List<VehicleSnapshot>> ListVehiclesAsync(IReadOnlyCollection<ulong> ids, CancellationToken ct = default);

    Task<List<VehicleSnapshot>> ListVehiclesByPublicIdsAsync(IReadOnlyCollection<Guid> publicIds,
        CancellationToken ct = default);

    Task<List<VehicleSnapshot>> ListVehiclesOfCarrierAsync(ulong carrierProfileId, CancellationToken ct = default);

    Task<DriverSnapshot?> FindDriverAsync(ulong driverId, CancellationToken ct = default);

    Task<ContactSnapshot?> FindContactAsync(ulong userId, CancellationToken ct = default);

    Task<ulong?> FindUserIdByPublicIdAsync(Guid publicId, CancellationToken ct = default);
}

public class IamContextFacade(
    IUserRepository userRepository,
    IPersonIdentityRepository identityRepository,
    ICarrierProfileRepository carrierProfileRepository,
    IMerchantProfileRepository merchantProfileRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    IPersonalDataProtector dataProtector) : IIamContextFacade
{
    public async Task<ulong?> FindCarrierProfileIdAsync(ulong userId, CancellationToken ct = default) =>
        (await carrierProfileRepository.FindByUserIdAsync(userId, ct))?.Id;

    public async Task<ulong?> FindMerchantProfileIdAsync(ulong userId, CancellationToken ct = default) =>
        (await merchantProfileRepository.FindByUserIdAsync(userId, ct))?.Id;

    public async Task<ulong?> FindUserIdByCarrierProfileIdAsync(ulong carrierProfileId, CancellationToken ct = default) =>
        (await carrierProfileRepository.FindByIdAsync(carrierProfileId, ct))?.UserId;

    public async Task<ulong?> FindUserIdByMerchantProfileIdAsync(ulong merchantProfileId,
        CancellationToken ct = default) =>
        (await merchantProfileRepository.FindByIdAsync(merchantProfileId, ct))?.UserId;

    public async Task<VehicleSnapshot?> FindVehicleAsync(Guid publicId, CancellationToken ct = default)
    {
        var vehicle = await vehicleRepository.FindByPublicIdAsync(publicId, ct);
        return vehicle is null
            ? null
            : new VehicleSnapshot(vehicle.Id, vehicle.PublicId, vehicle.CarrierProfileId, vehicle.Plate, vehicle.Model,
                vehicle.MaxLoadKg, vehicle.CargoVolumeM3, vehicle.IsActive);
    }

    public async Task<List<VehicleSnapshot>> ListVehiclesAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0) return [];
        var vehicles = await vehicleRepository.ListByIdsAsync(ids, ct);
        return vehicles.Select(vehicle => new VehicleSnapshot(vehicle.Id, vehicle.PublicId, vehicle.CarrierProfileId,
            vehicle.Plate, vehicle.Model, vehicle.MaxLoadKg, vehicle.CargoVolumeM3, vehicle.IsActive)).ToList();
    }

    public async Task<List<VehicleSnapshot>> ListVehiclesOfCarrierAsync(ulong carrierProfileId,
        CancellationToken ct = default) =>
        (await vehicleRepository.ListByCarrierAsync(carrierProfileId, ct))
        .Select(vehicle => new VehicleSnapshot(vehicle.Id, vehicle.PublicId, vehicle.CarrierProfileId, vehicle.Plate,
            vehicle.Model, vehicle.MaxLoadKg, vehicle.CargoVolumeM3, vehicle.IsActive))
        .ToList();

    public async Task<List<VehicleSnapshot>> ListVehiclesByPublicIdsAsync(IReadOnlyCollection<Guid> publicIds,
        CancellationToken ct = default)
    {
        var result = new List<VehicleSnapshot>();
        foreach (var publicId in publicIds.Distinct())
        {
            var vehicle = await FindVehicleAsync(publicId, ct);
            if (vehicle is not null) result.Add(vehicle);
        }

        return result;
    }

    public async Task<DriverSnapshot?> FindDriverAsync(ulong driverId, CancellationToken ct = default)
    {
        var driver = await driverRepository.FindByIdAsync(driverId, ct);
        return driver is null
            ? null
            : new DriverSnapshot(driver.Id, driver.CarrierProfileId, driver.FullName, driver.ExperienceStars,
                driver.IsActive);
    }

    public async Task<ContactSnapshot?> FindContactAsync(ulong userId, CancellationToken ct = default)
    {
        var user = await userRepository.FindByIdAsync(userId, ct);
        if (user is null) return null;
        var identity = await identityRepository.FindByUserIdAsync(userId, ct);
        var merchant = await merchantProfileRepository.FindByUserIdAsync(userId, ct);
        return new ContactSnapshot(user.Id, user.PublicId, identity?.FullName ?? user.Email,
            identity is null ? null : dataProtector.Decrypt(identity.PhoneEncrypted), merchant?.BusinessName);
    }

    public async Task<ulong?> FindUserIdByPublicIdAsync(Guid publicId, CancellationToken ct = default) =>
        (await userRepository.FindByPublicIdAsync(publicId, ct))?.Id;
}
