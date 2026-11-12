using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Trazza.Platform.Matchmaking.Application.CommandServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;
using Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Shipment requests")]
public class ShipmentRequestsController(
    IShipmentRequestCommandService shipmentRequestCommandService,
    IShipmentRequestQueryService shipmentRequestQueryService,
    IMatchProposalQueryService matchProposalQueryService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Create shipment request")]
    [EndpointDescription("A merchant asks to move cargo from a pickup point to a delivery point.")]
    [ProducesResponseType(typeof(ShipmentRequestResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateShipmentRequest([FromBody] CreateShipmentRequestResource resource,
        CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var command = CreateShipmentRequestCommandFromResourceAssembler.ToCommandFromResource(userId, resource);
        var result = await shipmentRequestCommandService.Handle(command, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            created => CreatedAtAction(nameof(GetShipmentRequestById),
                new { shipmentRequestId = created.Id.Value },
                ShipmentRequestResourceFromEntityAssembler.ToResourceFromEntity(created)));
    }

    [HttpGet("{shipmentRequestId}")]
    [EndpointSummary("Get shipment request by id")]
    [ProducesResponseType(typeof(ShipmentRequestResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShipmentRequestById(ulong shipmentRequestId,
        CancellationToken cancellationToken)
    {
        var request = await shipmentRequestQueryService.Handle(
            new GetShipmentRequestByIdQuery(new ShipmentRequestId(shipmentRequestId)), cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromEntity(
            this,
            request,
            MatchmakingErrors.ShipmentRequestNotFound,
            problemDetailsFactory,
            found => Ok(ShipmentRequestResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }

    [HttpGet("mine")]
    [EndpointSummary("Get my shipment requests")]
    [EndpointDescription("Gets the shipment requests of the merchant who makes the request.")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentRequestResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyShipmentRequests(CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var requests = await shipmentRequestQueryService.Handle(new GetShipmentRequestsByMerchantQuery(userId),
            cancellationToken);
        return Ok(requests.Select(ShipmentRequestResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("search")]
    [EndpointSummary("Search open shipment requests")]
    [EndpointDescription(
        "Finds open requests whose pickup is near the origin, whose delivery is near the destination and whose pickup window includes the departure.")]
    [ProducesResponseType(typeof(IEnumerable<ShipmentRequestResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchOpenShipmentRequests(
        [FromQuery] double originLatitude, [FromQuery] double originLongitude,
        [FromQuery] double destinationLatitude, [FromQuery] double destinationLongitude,
        [FromQuery] DateTimeOffset departureAt, [FromQuery] double? maxDistanceKm,
        CancellationToken cancellationToken)
    {
        var origin = new GeoLocation(originLatitude, originLongitude);
        var destination = new GeoLocation(destinationLatitude, destinationLongitude);

        if (!origin.IsValid || !destination.IsValid)
            return problemDetailsFactory.CreateProblemDetails(this, MatchmakingErrors.InvalidAddress);

        var query = new SearchOpenShipmentRequestsQuery(origin, destination, departureAt,
            maxDistanceKm ?? SearchReturnRoutesQuery.DefaultMaxDistanceKm);
        var requests = await shipmentRequestQueryService.Handle(query, cancellationToken);
        return Ok(requests.Select(ShipmentRequestResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{shipmentRequestId}/proposals")]
    [EndpointSummary("Get proposals of a shipment request")]
    [ProducesResponseType(typeof(IEnumerable<MatchProposalResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProposalsByShipmentRequest(ulong shipmentRequestId,
        CancellationToken cancellationToken)
    {
        var proposals = await matchProposalQueryService.Handle(
            new GetProposalsByShipmentQuery(new ShipmentRequestId(shipmentRequestId)), cancellationToken);
        return Ok(proposals.Select(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost("{shipmentRequestId}/cancel")]
    [EndpointSummary("Cancel shipment request")]
    [ProducesResponseType(typeof(ShipmentRequestResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelShipmentRequest(ulong shipmentRequestId, [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await shipmentRequestCommandService.Handle(
            new CancelShipmentRequestCommand(userId, new ShipmentRequestId(shipmentRequestId), reason),
            cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            cancelled => Ok(ShipmentRequestResourceFromEntityAssembler.ToResourceFromEntity(cancelled)));
    }
}
