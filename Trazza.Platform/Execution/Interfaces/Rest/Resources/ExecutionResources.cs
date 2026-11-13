namespace Trazza.Platform.Execution.Interfaces.Rest.Resources;

public record TripPlaceResource(string Address, double Latitude, double Longitude);

public record TripPartyResource(string Name, string? Phone);

public record TripVehicleResource(Guid Id, string Plate);

public record TrackingPointResource(
    ulong Id,
    double Latitude,
    double Longitude,
    decimal? SpeedKmh,
    string Source,
    DateTime RecordedAt);

public record TripResource(
    Guid Id,
    string Status,
    string MyRole,
    decimal AgreedRate,
    string Currency,
    ulong MatchProposalId,
    ulong ReturnRouteId,
    ulong ShipmentRequestId,
    string Description,
    decimal WeightKg,
    decimal VolumeM3,
    TripPlaceResource Origin,
    TripPlaceResource Destination,
    TripPlaceResource Pickup,
    TripPlaceResource Delivery,
    DateTime DepartureAt,
    TripPartyResource Carrier,
    TripPartyResource Merchant,
    TripVehicleResource Vehicle,
    ulong? DriverId,
    string? DriverName,
    TrackingPointResource? LastPoint,
    int OpenAlerts,
    decimal? TotalDistanceKm,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    DateTime CreatedAt);

public record TripStatusChangeResource(string? FromStatus, string ToStatus, DateTime ChangedAt);

public record AssignDriverResource(ulong DriverId);

public record CancelTripResource(string Reason);

public record RecordTrackingPointResource(
    double Latitude,
    double Longitude,
    decimal? SpeedKmh,
    string? Source,
    DateTime? RecordedAt);

public record DeviationAlertResource(
    ulong Id,
    double Latitude,
    double Longitude,
    decimal DeviationKm,
    DateTime DetectedAt,
    DateTime? AcknowledgedAt);

public record TrackingResultResource(TrackingPointResource Point, DeviationAlertResource? Alert, string TripStatus);

public record ReportIncidentResource(string Type, string Description);

public record AddIncidentEvidenceResource(string S3Key);

public record ResolveIncidentResource(string Resolution);

public record IncidentEvidenceResource(ulong Id, string S3Key, DateTime UploadedAt);

public record IncidentResource(
    ulong Id,
    string Type,
    string Description,
    string Status,
    string? Resolution,
    DateTime ReportedAt,
    DateTime? ResolvedAt,
    List<IncidentEvidenceResource> Evidences);

public record IotDeviceResource(ulong Id, string SerialNumber, string Status, Guid VehicleId, string VehiclePlate,
    DateTime? LoanedAt);
