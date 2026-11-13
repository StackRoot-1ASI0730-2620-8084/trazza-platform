using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Domain.Repositories;

public interface IReturnRouteRepository : IBaseRepository<ReturnRoute>
{
    Task<IEnumerable<ReturnRoute>> FindByCarrierProfileIdAsNoTrackingAsync(CarrierProfileId carrierProfileId,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Finds a route and locks its row (SELECT ... FOR UPDATE) until the transaction ends. It is used when
    ///     accepting a proposal, so two merchants cannot take the last free place of a route at the same time.
    /// </summary>
    Task<ReturnRoute?> FindByIdForUpdateAsync(ReturnRouteId returnRouteId, CancellationToken cancellationToken);

    /// <summary>
    ///     Finds published routes that start and end near the given points and leave inside the window.
    ///     The distance filter uses the spatial columns of the database.
    /// </summary>
    Task<IEnumerable<ReturnRoute>> SearchPublishedAsNoTrackingAsync(GeoLocation pickup, GeoLocation delivery,
        TimeWindow pickupWindow, double maxDistanceKm, CancellationToken cancellationToken);

    /// <summary>
    ///     Id of the route with the given public id, or null when there is none.
    /// </summary>
    Task<ReturnRouteId?> FindIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);

    /// <summary>
    ///     Public ids of the given entities, by id. It reads only the two id columns.
    /// </summary>
    Task<IReadOnlyDictionary<ulong, Guid>> FindPublicIdsAsync(IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken);
}
