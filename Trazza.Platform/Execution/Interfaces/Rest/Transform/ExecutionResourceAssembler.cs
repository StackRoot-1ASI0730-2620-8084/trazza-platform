using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Execution.Interfaces.Rest.Resources;

namespace Trazza.Platform.Execution.Interfaces.Rest.Transform;

public static class ExecutionResourceAssembler
{
    public static TripResource ToResource(TripView view)
    {
        var trip = view.Trip;
        var context = view.Context;
        var showPhones = !trip.IsClosed;
        return new TripResource(
            trip.PublicId,
            EnumText.ToText(trip.Status),
            view.IsCarrier ? Roles.Carrier : Roles.Merchant,
            trip.AgreedRate,
            trip.Currency,
            context.MatchProposalPublicId,
            context.ReturnRoutePublicId,
            context.ShipmentRequestPublicId,
            context.Description,
            context.WeightKg,
            context.VolumeM3,
            Place(context.OriginAddress, context.Origin),
            Place(context.DestinationAddress, context.Destination),
            Place(context.PickupAddress, context.Pickup),
            Place(context.DeliveryAddress, context.Delivery),
            context.DepartureAt,
            new TripPartyResource(view.CarrierName, showPhones ? view.CarrierPhone : null),
            new TripPartyResource(view.MerchantName, showPhones ? view.MerchantPhone : null),
            new TripVehicleResource(view.VehiclePublicId, view.VehiclePlate),
            trip.DriverId ?? context.RouteDriverId,
            view.DriverName,
            view.LastPoint is null ? null : ToResource(view.LastPoint),
            view.OpenAlerts,
            trip.TotalDistanceKm,
            trip.PickedUpAt,
            trip.DeliveredAt,
            trip.ConfirmedAt,
            trip.CancelledAt,
            trip.CancellationReason,
            trip.CreatedAt);
    }

    public static TripStatusChangeResource ToResource(TripStatusChange change) =>
        new(change.FromStatus, change.ToStatus, change.ChangedAt);

    public static TrackingPointResource ToResource(TrackingPoint point) =>
        new(point.Id, point.Latitude, point.Longitude, point.SpeedKmh, EnumText.ToText(point.Source), point.RecordedAt);

    public static DeviationAlertResource ToResource(DeviationAlert alert) =>
        new(alert.Id, alert.Latitude, alert.Longitude, alert.DeviationKm, alert.DetectedAt, alert.AcknowledgedAt);

    public static TrackingResultResource ToResource(TrackingResult result) =>
        new(ToResource(result.Point), result.Alert is null ? null : ToResource(result.Alert), result.TripStatus);

    public static IncidentResource ToResource(Incident incident) =>
        new(incident.Id, EnumText.ToText(incident.Type), incident.Description, EnumText.ToText(incident.Status),
            incident.Resolution, incident.ReportedAt, incident.ResolvedAt,
            incident.Evidences.Select(evidence => new IncidentEvidenceResource(evidence.Id, evidence.S3Key,
                evidence.UploadedAt)).ToList());

    public static IotDeviceResource ToResource(IotDeviceView view) =>
        new(view.Device.Id, view.Device.SerialNumber, EnumText.ToText(view.Device.Status), view.VehiclePublicId,
            view.VehiclePlate, view.Device.LoanedAt);

    private static TripPlaceResource Place(string address, GeoLocation location) =>
        new(address, location.Latitude, location.Longitude);
}
