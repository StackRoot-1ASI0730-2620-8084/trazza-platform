using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Domain.Repositories;

public interface IShipmentRequestRepository : IBaseRepository<ShipmentRequest>
{
    Task<IEnumerable<ShipmentRequest>> FindByMerchantProfileIdAsNoTrackingAsync(MerchantProfileId merchantProfileId,
        CancellationToken cancellationToken);

    Task<IEnumerable<ShipmentRequest>> SearchOpenAsNoTrackingAsync(GeoLocation origin, GeoLocation destination,
        DateTimeOffset departureAt, double maxDistanceKm, CancellationToken cancellationToken);

    Task<ShipmentRequestId?> FindIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<ulong, Guid>> FindPublicIdsAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken);

    Task<IEnumerable<ShipmentRequest>> ListOpenAsNoTrackingAsync(DateTimeOffset now,
        CancellationToken cancellationToken);
}
