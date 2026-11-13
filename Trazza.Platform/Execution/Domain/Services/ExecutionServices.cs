using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Model.Commands;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Execution.Domain.Services;

public record TripView(
    Trip Trip,
    TripContextSnapshot Context,
    ulong CarrierUserId,
    string CarrierName,
    string? CarrierPhone,
    ulong MerchantUserId,
    string MerchantName,
    string? MerchantPhone,
    Guid VehiclePublicId,
    string VehiclePlate,
    string? DriverName,
    TrackingPoint? LastPoint,
    int OpenAlerts,
    bool IsCarrier);

public record TripContextSnapshot(
    ulong MatchProposalId,
    Guid MatchProposalPublicId,
    Guid ReturnRoutePublicId,
    Guid ShipmentRequestPublicId,
    ulong CarrierProfileId,
    ulong MerchantProfileId,
    ulong VehicleId,
    ulong? RouteDriverId,
    string OriginAddress,
    GeoLocation Origin,
    string DestinationAddress,
    GeoLocation Destination,
    string PickupAddress,
    GeoLocation Pickup,
    string DeliveryAddress,
    GeoLocation Delivery,
    DateTime DepartureAt,
    string Description,
    decimal WeightKg,
    decimal VolumeM3);

public record TrackingResult(TrackingPoint Point, DeviationAlert? Alert, string TripStatus);

public record IotDeviceView(IotDevice Device, Guid VehiclePublicId, string VehiclePlate);

public interface ITripCommandService
{
    Task<Result<TripView, CommandError>> Handle(AssignDriverCommand command, CancellationToken ct = default);

    Task<Result<TripView, CommandError>> Handle(PickUpTripCommand command, CancellationToken ct = default);

    Task<Result<TripView, CommandError>> Handle(DeliverTripCommand command, CancellationToken ct = default);

    Task<Result<TripView, CommandError>> Handle(ConfirmTripCommand command, CancellationToken ct = default);

    Task<Result<TripView, CommandError>> Handle(CancelTripCommand command, CancellationToken ct = default);
}

public interface ITrackingCommandService
{
    Task<Result<TrackingResult, CommandError>> Handle(RecordTrackingPointCommand command, CancellationToken ct = default);

    Task<Result<bool, CommandError>> Handle(AcknowledgeDeviationAlertCommand command, CancellationToken ct = default);
}

public interface IIncidentCommandService
{
    Task<Result<Incident, CommandError>> Handle(ReportIncidentCommand command, CancellationToken ct = default);

    Task<Result<Incident, CommandError>> Handle(AddIncidentEvidenceCommand command, CancellationToken ct = default);

    Task<Result<Incident, CommandError>> Handle(ResolveIncidentCommand command, CancellationToken ct = default);
}

public interface IExecutionQueryService
{
    Task<List<TripView>> ListMyTripsAsync(ulong userId, CancellationToken ct = default);

    Task<Result<TripView, CommandError>> GetTripAsync(ulong userId, Guid tripId, CancellationToken ct = default);

    Task<Result<List<TripStatusChange>, CommandError>> ListHistoryAsync(ulong userId, Guid tripId,
        CancellationToken ct = default);

    Task<Result<List<TrackingPoint>, CommandError>> ListTrackingPointsAsync(ulong userId, Guid tripId,
        CancellationToken ct = default);

    Task<Result<List<DeviationAlert>, CommandError>> ListDeviationAlertsAsync(ulong userId, Guid tripId,
        CancellationToken ct = default);

    Task<Result<List<Incident>, CommandError>> ListIncidentsAsync(ulong userId, Guid tripId, CancellationToken ct = default);

    Task<List<IotDeviceView>> ListMyIotDevicesAsync(ulong userId, CancellationToken ct = default);
}
