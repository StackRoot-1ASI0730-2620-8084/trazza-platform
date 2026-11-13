using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Infrastructure.Profiles;

/// <summary>
///     Reads carrier and merchant profiles, vehicles and drivers from the IAM context, which owns them.
/// </summary>
public class IamProfilesService(IIamContextFacade iamContextFacade) : IProfilesService
{
    public async Task<CarrierProfileId?> FetchCarrierProfileIdByUserId(UserId userId,
        CancellationToken cancellationToken) =>
        await iamContextFacade.FindCarrierProfileIdAsync(userId.Value, cancellationToken) is { } id
            ? new CarrierProfileId(id)
            : null;

    public async Task<MerchantProfileId?> FetchMerchantProfileIdByUserId(UserId userId,
        CancellationToken cancellationToken) =>
        await iamContextFacade.FindMerchantProfileIdAsync(userId.Value, cancellationToken) is { } id
            ? new MerchantProfileId(id)
            : null;

    public async Task<decimal?> FetchVehicleMaxLoadKg(CarrierProfileId carrierProfileId, VehicleId vehicleId,
        CancellationToken cancellationToken)
    {
        var vehicles = await iamContextFacade.ListVehiclesAsync([vehicleId.Value], cancellationToken);
        var vehicle = vehicles.FirstOrDefault(v => v.CarrierProfileId == carrierProfileId.Value && v.IsActive);
        return vehicle?.MaxLoadKg;
    }

    public async Task<bool> OwnsDriver(CarrierProfileId carrierProfileId, DriverId driverId,
        CancellationToken cancellationToken)
    {
        var driver = await iamContextFacade.FindDriverAsync(driverId.Value, cancellationToken);
        return driver is { IsActive: true } && driver.CarrierProfileId == carrierProfileId.Value;
    }

    public async Task<VehicleId?> FetchVehicleIdByPublicId(Guid publicId, CancellationToken cancellationToken) =>
        await iamContextFacade.FindVehicleAsync(publicId, cancellationToken) is { } vehicle
            ? new VehicleId(vehicle.Id)
            : null;

    public async Task<IReadOnlyDictionary<VehicleId, Guid>> FetchVehiclePublicIds(
        IReadOnlyCollection<VehicleId> vehicleIds, CancellationToken cancellationToken)
    {
        var ids = vehicleIds.Select(id => id.Value).Distinct().ToList();
        var vehicles = await iamContextFacade.ListVehiclesAsync(ids, cancellationToken);
        return vehicles.ToDictionary(vehicle => new VehicleId(vehicle.Id), vehicle => vehicle.PublicId);
    }
}
