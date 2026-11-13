using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;

/// <summary>
///     Port to the Profiles context. Matchmaking does not know how Profiles stores carriers, merchants or vehicles.
/// </summary>
public interface IProfilesService
{
    Task<CarrierProfileId?> FetchCarrierProfileIdByUserId(UserId userId, CancellationToken cancellationToken);

    Task<MerchantProfileId?> FetchMerchantProfileIdByUserId(UserId userId, CancellationToken cancellationToken);

    /// <summary>
    ///     Maximum load in kilograms of a vehicle, or null when the vehicle does not belong to the carrier.
    /// </summary>
    Task<decimal?> FetchVehicleMaxLoadKg(CarrierProfileId carrierProfileId, VehicleId vehicleId,
        CancellationToken cancellationToken);

    Task<bool> OwnsDriver(CarrierProfileId carrierProfileId, DriverId driverId, CancellationToken cancellationToken);

    /// <summary>
    ///     Internal id of the vehicle with the given public id, or null when it does not exist.
    /// </summary>
    Task<VehicleId?> FetchVehicleIdByPublicId(Guid publicId, CancellationToken cancellationToken);

    /// <summary>
    ///     Public ids of the given vehicles, keyed by their internal id. Missing vehicles are left out.
    /// </summary>
    Task<IReadOnlyDictionary<VehicleId, Guid>> FetchVehiclePublicIds(IReadOnlyCollection<VehicleId> vehicleIds,
        CancellationToken cancellationToken);
}
