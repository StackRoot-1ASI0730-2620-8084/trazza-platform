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
}
