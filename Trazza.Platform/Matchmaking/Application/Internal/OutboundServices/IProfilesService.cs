using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;

public interface IProfilesService
{
    Task<CarrierProfileId?> FetchCarrierProfileIdByUserId(UserId userId, CancellationToken cancellationToken);

    Task<MerchantProfileId?> FetchMerchantProfileIdByUserId(UserId userId, CancellationToken cancellationToken);

    Task<decimal?> FetchVehicleMaxLoadKg(CarrierProfileId carrierProfileId, VehicleId vehicleId,
        CancellationToken cancellationToken);

    Task<bool> OwnsDriver(CarrierProfileId carrierProfileId, DriverId driverId, CancellationToken cancellationToken);

    Task<VehicleId?> FetchVehicleIdByPublicId(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<VehicleId, Guid>> FetchVehiclePublicIds(IReadOnlyCollection<VehicleId> vehicleIds,
        CancellationToken cancellationToken);
}
