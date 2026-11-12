using TrazzaPlatform.API.Iam.Domain.Model.Aggregates;
using TrazzaPlatform.API.Iam.Domain.Repositories;
using TrazzaPlatform.API.Iam.Domain.Services;

namespace TrazzaPlatform.API.Iam.Application.Internal.QueryServices;

public class FleetQueryService(
    ICarrierProfileRepository carrierProfileRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    ISavedAddressRepository savedAddressRepository) : IFleetQueryService
{
    public async Task<List<Vehicle>> ListVehiclesAsync(ulong userId, CancellationToken ct = default)
    {
        var profile = await carrierProfileRepository.FindByUserIdAsync(userId, ct);
        return profile is null ? [] : await vehicleRepository.ListByCarrierAsync(profile.Id, ct);
    }

    public async Task<List<Driver>> ListDriversAsync(ulong userId, CancellationToken ct = default)
    {
        var profile = await carrierProfileRepository.FindByUserIdAsync(userId, ct);
        return profile is null ? [] : await driverRepository.ListByCarrierAsync(profile.Id, ct);
    }

    public Task<List<SavedAddress>> ListSavedAddressesAsync(ulong userId, CancellationToken ct = default) =>
        savedAddressRepository.ListByUserAsync(userId, ct);
}
