using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Infrastructure.Profiles;

/// <summary>
///     Temporary stand-in for the Profiles context, which does not exist yet. Every user is both a carrier
///     and a merchant, the profile id equals the user id, and every vehicle and driver belongs to the caller.
///     Replace it with an adapter to the Profiles facade when that context is merged.
/// </summary>
public class FakeProfilesService : IProfilesService
{
    private const decimal VehicleMaxLoadKg = 20000m;

    public Task<CarrierProfileId?> FetchCarrierProfileIdByUserId(UserId userId, CancellationToken cancellationToken)
    {
        return Task.FromResult<CarrierProfileId?>(new CarrierProfileId(userId.Value));
    }

    public Task<MerchantProfileId?> FetchMerchantProfileIdByUserId(UserId userId, CancellationToken cancellationToken)
    {
        return Task.FromResult<MerchantProfileId?>(new MerchantProfileId(userId.Value));
    }

    public Task<decimal?> FetchVehicleMaxLoadKg(CarrierProfileId carrierProfileId, VehicleId vehicleId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<decimal?>(VehicleMaxLoadKg);
    }

    public Task<bool> OwnsDriver(CarrierProfileId carrierProfileId, DriverId driverId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}
