using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Execution.Domain.Repositories;

public interface ITripRepository : IBaseRepository<Trip>
{
    Task<Trip?> FindByPublicIdAsync(Guid publicId, bool includeHistory = false, CancellationToken cancellationToken = default);

    Task<Trip?> FindByProposalIdAsync(ulong matchProposalId, CancellationToken cancellationToken = default);

    Task<List<Trip>> ListByProposalIdsAsync(IReadOnlyCollection<ulong> matchProposalIds,
        CancellationToken cancellationToken = default);
}

public interface ITrackingPointRepository
{
    Task<TrackingPoint> AddAsync(TrackingPoint point, CancellationToken cancellationToken = default);

    Task<List<TrackingPoint>> ListByTripAsync(ulong tripId, CancellationToken cancellationToken = default);

    Task<TrackingPoint?> FindLastAsync(ulong tripId, CancellationToken cancellationToken = default);
}

public interface IDeviationAlertRepository
{
    Task<DeviationAlert> AddAsync(DeviationAlert alert, CancellationToken cancellationToken = default);

    Task<List<DeviationAlert>> ListByTripAsync(ulong tripId, CancellationToken cancellationToken = default);

    Task<DeviationAlert?> FindLastOpenAsync(ulong tripId, CancellationToken cancellationToken = default);

    Task<bool> AcknowledgeAsync(ulong alertId, ulong tripId, DateTime now, CancellationToken cancellationToken = default);
}

public interface IIncidentRepository : IBaseRepository<Incident>
{
    Task<List<Incident>> ListByTripAsync(ulong tripId, CancellationToken cancellationToken = default);

    Task<Incident?> FindWithEvidencesAsync(ulong incidentId, ulong tripId, CancellationToken cancellationToken = default);
}

public interface IIotDeviceRepository : IBaseRepository<IotDevice>
{
    Task<List<IotDevice>> ListAvailableAsync(int count, CancellationToken cancellationToken = default);

    Task<List<IotDevice>> ListByVehicleIdsAsync(IReadOnlyCollection<ulong> vehicleIds,
        CancellationToken cancellationToken = default);
}
