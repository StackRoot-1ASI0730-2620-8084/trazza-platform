using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Trazza.Platform.Matchmaking.Application.CommandServices;
using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
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
[Tags("Return routes")]
public class ReturnRoutesController(
    IReturnRouteCommandService returnRouteCommandService,
    IReturnRouteQueryService returnRouteQueryService,
    IMatchProposalQueryService matchProposalQueryService,
    IProfilesService profilesService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Publish return route")]
    [EndpointDescription("A carrier publishes the route of its vehicle on the way back, so merchants can offer cargo.")]
    [ProducesResponseType(typeof(ReturnRouteResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishReturnRoute([FromBody] PublishReturnRouteResource resource,
        CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        // The API exposes the public id of the vehicle; an unknown id fails as a vehicle the carrier does not own.
        var vehicleId = await profilesService.FetchVehicleIdByPublicId(resource.VehicleId, cancellationToken) ??
                        new VehicleId(0);
        var command = PublishReturnRouteCommandFromResourceAssembler.ToCommandFromResource(userId, vehicleId, resource);
        var result = await returnRouteCommandService.Handle(command, cancellationToken);
        var vehiclePublicIds = await FetchVehiclePublicIds(result.IsSuccess ? [result.Value] : [], cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            published => CreatedAtAction(nameof(GetReturnRouteById), new { returnRouteId = published.Id.Value },
                ReturnRouteResourceFromEntityAssembler.ToResourceFromEntity(published, vehiclePublicIds)));
    }

    [HttpGet("{returnRouteId}")]
    [EndpointSummary("Get return route by id")]
    [ProducesResponseType(typeof(ReturnRouteResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReturnRouteById(ulong returnRouteId, CancellationToken cancellationToken)
    {
        var route = await returnRouteQueryService.Handle(new GetReturnRouteByIdQuery(new ReturnRouteId(returnRouteId)),
            cancellationToken);
        var vehiclePublicIds = await FetchVehiclePublicIds(route is null ? [] : [route], cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromEntity(
            this,
            route,
            MatchmakingErrors.ReturnRouteNotFound,
            problemDetailsFactory,
            found => Ok(ReturnRouteResourceFromEntityAssembler.ToResourceFromEntity(found, vehiclePublicIds)));
    }

    [HttpGet("mine")]
    [EndpointSummary("Get my return routes")]
    [EndpointDescription("Gets the routes published by the carrier who makes the request.")]
    [ProducesResponseType(typeof(IEnumerable<ReturnRouteResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReturnRoutes(CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var routes = await returnRouteQueryService.Handle(new GetReturnRoutesByCarrierQuery(userId),
            cancellationToken);
        return Ok(await ToResources(routes, cancellationToken));
    }

    [HttpGet("search")]
    [EndpointSummary("Search return routes")]
    [EndpointDescription(
        "Finds published routes that start near the pickup, end near the delivery and leave inside the pickup window.")]
    [ProducesResponseType(typeof(IEnumerable<ReturnRouteResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchReturnRoutes(
        [FromQuery] double pickupLatitude, [FromQuery] double pickupLongitude,
        [FromQuery] double deliveryLatitude, [FromQuery] double deliveryLongitude,
        [FromQuery] DateTimeOffset pickupFrom, [FromQuery] DateTimeOffset pickupUntil,
        [FromQuery] double? maxDistanceKm, CancellationToken cancellationToken)
    {
        var pickup = new GeoLocation(pickupLatitude, pickupLongitude);
        var delivery = new GeoLocation(deliveryLatitude, deliveryLongitude);
        var window = new TimeWindow(pickupFrom, pickupUntil);

        if (!pickup.IsValid || !delivery.IsValid)
            return problemDetailsFactory.CreateProblemDetails(this, MatchmakingErrors.InvalidAddress);
        if (!window.IsValid)
            return problemDetailsFactory.CreateProblemDetails(this, MatchmakingErrors.InvalidTimeWindow);

        var query = new SearchReturnRoutesQuery(pickup, delivery, window,
            maxDistanceKm ?? SearchReturnRoutesQuery.DefaultMaxDistanceKm);
        var routes = await returnRouteQueryService.Handle(query, cancellationToken);
        return Ok(await ToResources(routes, cancellationToken));
    }

    [HttpGet("{returnRouteId}/proposals")]
    [EndpointSummary("Get proposals of a return route")]
    [ProducesResponseType(typeof(IEnumerable<MatchProposalResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProposalsByReturnRoute(ulong returnRouteId,
        CancellationToken cancellationToken)
    {
        var proposals = await matchProposalQueryService.Handle(
            new GetProposalsByRouteQuery(new ReturnRouteId(returnRouteId)), cancellationToken);
        return Ok(proposals.Select(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost("{returnRouteId}/cancel")]
    [EndpointSummary("Cancel return route")]
    [ProducesResponseType(typeof(ReturnRouteResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelReturnRoute(ulong returnRouteId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await returnRouteCommandService.Handle(
            new CancelReturnRouteCommand(userId, new ReturnRouteId(returnRouteId)), cancellationToken);
        var vehiclePublicIds = await FetchVehiclePublicIds(result.IsSuccess ? [result.Value] : [], cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            cancelled => Ok(ReturnRouteResourceFromEntityAssembler.ToResourceFromEntity(cancelled, vehiclePublicIds)));
    }

    private Task<IReadOnlyDictionary<VehicleId, Guid>> FetchVehiclePublicIds(IEnumerable<ReturnRoute> routes,
        CancellationToken cancellationToken) =>
        profilesService.FetchVehiclePublicIds(routes.Select(route => route.VehicleId).ToList(), cancellationToken);

    private async Task<IEnumerable<ReturnRouteResource>> ToResources(IEnumerable<ReturnRoute> routes,
        CancellationToken cancellationToken)
    {
        var list = routes.ToList();
        var vehiclePublicIds = await FetchVehiclePublicIds(list, cancellationToken);
        return list.Select(route => ReturnRouteResourceFromEntityAssembler.ToResourceFromEntity(route, vehiclePublicIds));
    }
}
