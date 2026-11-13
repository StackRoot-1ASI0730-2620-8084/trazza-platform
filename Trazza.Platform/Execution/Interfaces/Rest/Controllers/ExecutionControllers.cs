using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Execution.Interfaces.Rest.Transform;
using Trazza.Platform.Execution.Domain.Model.Commands;
using Trazza.Platform.Execution.Domain.Model.ValueObjects;
using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Execution.Interfaces.Rest.Resources;
using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Iam.Infrastructure.Pipeline;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Interfaces.Rest;

namespace Trazza.Platform.Execution.Interfaces.Rest.Controllers;

/// <summary>
///     Trips of the signed-in user (as carrier or merchant): life cycle, GPS tracking, deviation alerts and incidents.
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class TripsController(
    ITripCommandService tripCommandService,
    ITrackingCommandService trackingCommandService,
    IIncidentCommandService incidentCommandService,
    IExecutionQueryService queryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;

    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken ct) =>
        Ok((await queryService.ListMyTripsAsync(UserId, ct)).Select(ExecutionResourceAssembler.ToResource));

    [HttpGet("{tripId:guid}")]
    public async Task<IActionResult> Get(Guid tripId, CancellationToken ct) =>
        FromResult(await queryService.GetTripAsync(UserId, tripId, ct), ExecutionResourceAssembler.ToResource);

    [HttpGet("{tripId:guid}/status-history")]
    public async Task<IActionResult> History(Guid tripId, CancellationToken ct) =>
        FromResult(await queryService.ListHistoryAsync(UserId, tripId, ct),
            changes => changes.Select(ExecutionResourceAssembler.ToResource).ToList());

    [HttpPut("{tripId:guid}/driver")]
    [Authorize(Roles.Carrier)]
    public async Task<IActionResult> AssignDriver(Guid tripId, [FromBody] AssignDriverResource resource,
        CancellationToken ct) =>
        FromResult(await tripCommandService.Handle(new AssignDriverCommand(UserId, tripId, resource.DriverId), ct),
            ExecutionResourceAssembler.ToResource);

    [HttpPost("{tripId:guid}/pick-up")]
    [Authorize(Roles.Carrier)]
    public async Task<IActionResult> PickUp(Guid tripId, CancellationToken ct) =>
        FromResult(await tripCommandService.Handle(new PickUpTripCommand(UserId, tripId), ct),
            ExecutionResourceAssembler.ToResource);

    [HttpPost("{tripId:guid}/deliver")]
    [Authorize(Roles.Carrier)]
    public async Task<IActionResult> Deliver(Guid tripId, CancellationToken ct) =>
        FromResult(await tripCommandService.Handle(new DeliverTripCommand(UserId, tripId), ct),
            ExecutionResourceAssembler.ToResource);

    [HttpPost("{tripId:guid}/confirm")]
    [Authorize(Roles.Merchant)]
    public async Task<IActionResult> Confirm(Guid tripId, CancellationToken ct) =>
        FromResult(await tripCommandService.Handle(new ConfirmTripCommand(UserId, tripId), ct),
            ExecutionResourceAssembler.ToResource);

    [HttpPost("{tripId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid tripId, [FromBody] CancelTripResource resource, CancellationToken ct) =>
        FromResult(await tripCommandService.Handle(new CancelTripCommand(UserId, tripId, resource.Reason), ct),
            ExecutionResourceAssembler.ToResource);

    [HttpGet("{tripId:guid}/tracking-points")]
    public async Task<IActionResult> ListTrackingPoints(Guid tripId, CancellationToken ct) =>
        FromResult(await queryService.ListTrackingPointsAsync(UserId, tripId, ct),
            points => points.Select(ExecutionResourceAssembler.ToResource).ToList());

    /// <summary>GPS point from the carrier's phone, the GPS simulator or an IoT device (source PHONE or IOT).</summary>
    [HttpPost("{tripId:guid}/tracking-points")]
    [Authorize(Roles.Carrier)]
    public async Task<IActionResult> RecordTrackingPoint(Guid tripId, [FromBody] RecordTrackingPointResource resource,
        CancellationToken ct)
    {
        var source = TrackingSource.Phone;
        if (!string.IsNullOrWhiteSpace(resource.Source) && !EnumText.TryParse(resource.Source, out source))
            return ErrorResult(CommandError.Validation("source-invalid"));
        return CreatedFromResult(await trackingCommandService.Handle(new RecordTrackingPointCommand(UserId, tripId,
                resource.Latitude, resource.Longitude, resource.SpeedKmh, source, resource.RecordedAt), ct),
            ExecutionResourceAssembler.ToResource);
    }

    [HttpGet("{tripId:guid}/deviation-alerts")]
    public async Task<IActionResult> ListDeviationAlerts(Guid tripId, CancellationToken ct) =>
        FromResult(await queryService.ListDeviationAlertsAsync(UserId, tripId, ct),
            alerts => alerts.Select(ExecutionResourceAssembler.ToResource).ToList());

    [HttpPost("{tripId:guid}/deviation-alerts/{alertId}/acknowledge")]
    public async Task<IActionResult> AcknowledgeDeviationAlert(Guid tripId, ulong alertId, CancellationToken ct) =>
        NoContentFromResult(await trackingCommandService.Handle(
            new AcknowledgeDeviationAlertCommand(UserId, tripId, alertId), ct));

    [HttpGet("{tripId:guid}/incidents")]
    public async Task<IActionResult> ListIncidents(Guid tripId, CancellationToken ct) =>
        FromResult(await queryService.ListIncidentsAsync(UserId, tripId, ct),
            incidents => incidents.Select(ExecutionResourceAssembler.ToResource).ToList());

    [HttpPost("{tripId:guid}/incidents")]
    public async Task<IActionResult> ReportIncident(Guid tripId, [FromBody] ReportIncidentResource resource,
        CancellationToken ct)
    {
        if (!EnumText.TryParse<IncidentType>(resource.Type, out var type))
            return ErrorResult(CommandError.Validation("incident-type-invalid"));
        return CreatedFromResult(await incidentCommandService.Handle(
                new ReportIncidentCommand(UserId, tripId, type, resource.Description), ct),
            ExecutionResourceAssembler.ToResource);
    }

    [HttpPost("{tripId:guid}/incidents/{incidentId}/evidences")]
    public async Task<IActionResult> AddIncidentEvidence(Guid tripId, ulong incidentId,
        [FromBody] AddIncidentEvidenceResource resource, CancellationToken ct) =>
        CreatedFromResult(await incidentCommandService.Handle(
                new AddIncidentEvidenceCommand(UserId, tripId, incidentId, resource.S3Key), ct),
            ExecutionResourceAssembler.ToResource);

    [HttpPost("{tripId:guid}/incidents/{incidentId}/resolve")]
    public async Task<IActionResult> ResolveIncident(Guid tripId, ulong incidentId,
        [FromBody] ResolveIncidentResource resource, CancellationToken ct) =>
        FromResult(await incidentCommandService.Handle(
                new ResolveIncidentCommand(UserId, tripId, incidentId, resource.Resolution), ct),
            ExecutionResourceAssembler.ToResource);
}

/// <summary>IoT cargo sensors lent to the signed-in carrier's trucks.</summary>
[Route("api/v1/[controller]")]
[Authorize(Roles.Carrier)]
public class IotDevicesController(IExecutionQueryService queryService, IStringLocalizer<SharedResource> localizer)
    : TrazzaControllerBase(localizer)
{
    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken ct) =>
        Ok((await queryService.ListMyIotDevicesAsync(HttpContext.GetAuthenticatedUser().Id, ct))
            .Select(ExecutionResourceAssembler.ToResource));
}
