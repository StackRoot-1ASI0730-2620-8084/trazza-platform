using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Domain.Repositories;

public interface IReturnRouteRepository : IBaseRepository<ReturnRoute>
{
    Task<IEnumerable<ReturnRoute>> FindByCarrierProfileIdAsNoTrackingAsync(CarrierProfileId carrierProfileId,
        CancellationToken cancellationToken);

    Task<ReturnRoute?> FindByIdForUpdateAsync(ReturnRouteId returnRouteId, CancellationToken cancellationToken);

    Task<IEnumerable<ReturnRoute>> SearchPublishedAsNoTrackingAsync(GeoLocation pickup, GeoLocation delivery,
        TimeWindow pickupWindow, double maxDistanceKm, CancellationToken cancellationToken);

    Task<ReturnRouteId?> FindIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<ulong, Guid>> FindPublicIdsAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken);
}
