using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Domain.Repositories;

public interface IShipmentRequestRepository : IBaseRepository<ShipmentRequest>
{
    Task<IEnumerable<ShipmentRequest>> FindByMerchantProfileIdAsNoTrackingAsync(MerchantProfileId merchantProfileId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Finds open requests whose pickup is near the origin, whose delivery is near the destination,
    ///     and whose pickup window includes the departure.
    /// </summary>
    Task<IEnumerable<ShipmentRequest>> SearchOpenAsNoTrackingAsync(GeoLocation origin, GeoLocation destination,
        DateTimeOffset departureAt, double maxDistanceKm, CancellationToken cancellationToken);
}
