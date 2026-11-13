using Trazza.Platform.Execution.Domain.Model.ValueObjects;

namespace Trazza.Platform.Execution.Domain.Model.Commands;

public record AssignDriverCommand(ulong UserId, Guid TripId, ulong DriverId);

public record PickUpTripCommand(ulong UserId, Guid TripId);

public record DeliverTripCommand(ulong UserId, Guid TripId);

public record ConfirmTripCommand(ulong UserId, Guid TripId);

public record CancelTripCommand(ulong UserId, Guid TripId, string Reason);

public record RecordTrackingPointCommand(
    ulong UserId,
    Guid TripId,
    double Latitude,
    double Longitude,
    decimal? SpeedKmh,
    TrackingSource Source,
    DateTime? RecordedAt);

public record AcknowledgeDeviationAlertCommand(ulong UserId, Guid TripId, ulong AlertId);

public record ReportIncidentCommand(ulong UserId, Guid TripId, IncidentType Type, string Description);

public record AddIncidentEvidenceCommand(ulong UserId, Guid TripId, ulong IncidentId, string S3Key);

public record ResolveIncidentCommand(ulong UserId, Guid TripId, ulong IncidentId, string Resolution);