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

    /// <summary>
    ///     Id of the shipment request with the given public id, or null when there is none.
    /// </summary>
    Task<ShipmentRequestId?> FindIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    /// <summary>
    ///     Public ids of the given entities, by id. It reads only the two id columns.
    /// </summary>
    Task<IReadOnlyDictionary<ulong, Guid>> FindPublicIdsAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Open requests whose pickup window ends after the given time, the soonest first.
    /// </summary>
    Task<IEnumerable<ShipmentRequest>> ListOpenAsNoTrackingAsync(DateTimeOffset now,
        CancellationToken cancellationToken);
}
