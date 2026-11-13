using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Trazza.Platform.Execution.Interfaces.Acl;
using Trazza.Platform.Matchmaking.Application.CommandServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
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
[Tags("Match proposals")]
public class MatchProposalsController(
    IMatchProposalCommandService matchProposalCommandService,
    IMatchProposalQueryService matchProposalQueryService,
    IExecutionContextFacade executionContextFacade,
    IPublicIdQueryService publicIdQueryService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Submit match proposal")]
    [EndpointDescription("A carrier offers one of its routes, with a rate, to a shipment request.")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitMatchProposal([FromBody] SubmitMatchProposalResource resource,
        CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        // Unknown public ids fail as a request or route that does not exist.
        var shipmentRequestId =
            await publicIdQueryService.FindShipmentRequestIdAsync(resource.ShipmentRequestId, cancellationToken) ??
            new ShipmentRequestId(0);
        var returnRouteId =
            await publicIdQueryService.FindReturnRouteIdAsync(resource.ReturnRouteId, cancellationToken) ??
            new ReturnRouteId(0);
        var command = SubmitMatchProposalCommandFromResourceAssembler.ToCommandFromResource(userId,
            shipmentRequestId, returnRouteId, resource);
        var result = await matchProposalCommandService.Handle(command, cancellationToken);
        var publicIds = new ProposalPublicIds(
            new Dictionary<ulong, Guid> { [returnRouteId] = resource.ReturnRouteId },
            new Dictionary<ulong, Guid> { [shipmentRequestId] = resource.ShipmentRequestId });

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            submitted => CreatedAtAction(nameof(GetMatchProposalById), new { matchProposalId = submitted.PublicId },
                MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(submitted, publicIds)));
    }

    [HttpGet("{matchProposalId:guid}")]
    [EndpointSummary("Get match proposal by id")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatchProposalById(Guid matchProposalId, CancellationToken cancellationToken)
    {
        var id = await publicIdQueryService.FindMatchProposalIdAsync(matchProposalId, cancellationToken);
        var proposal = id is null
            ? null
            : await matchProposalQueryService.Handle(new GetMatchProposalByIdQuery(id.Value), cancellationToken);
        var tripId = proposal is null ? null : await FindTripIdAsync(proposal.Id, cancellationToken);
        var publicIds = await FetchPublicIds(proposal, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromEntity(
            this,
            proposal,
            MatchmakingErrors.MatchProposalNotFound,
            problemDetailsFactory,
            found => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(found, publicIds, tripId)));
    }

    [HttpPost("{matchProposalId:guid}/accept")]
    [EndpointSummary("Accept match proposal")]
    [EndpointDescription(
        "The merchant accepts the offer: the shipment request is matched and its other pending proposals expire.")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptMatchProposal(Guid matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var id = await FindIdAsync(matchProposalId, cancellationToken);
        var result = await matchProposalCommandService.Handle(new AcceptMatchProposalCommand(userId, id),
            cancellationToken);
        var tripId = result.IsSuccess ? await FindTripIdAsync(id, cancellationToken) : null;
        var publicIds = await FetchPublicIds(result.IsSuccess ? result.Value : null, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            accepted => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(accepted, publicIds, tripId)));
    }

    private async Task<Guid?> FindTripIdAsync(ulong matchProposalId, CancellationToken cancellationToken) =>
        (await executionContextFacade.FindTripPublicIdsByProposalIdsAsync([matchProposalId], cancellationToken))
        .TryGetValue(matchProposalId, out var tripId)
            ? tripId
            : null;

    /// <summary>
    ///     An unknown public id resolves to id 0, which the command services answer as a proposal not found.
    /// </summary>
    private async Task<MatchProposalId> FindIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        await publicIdQueryService.FindMatchProposalIdAsync(publicId, cancellationToken) ?? new MatchProposalId(0);

    private Task<ProposalPublicIds> FetchPublicIds(MatchProposal? proposal, CancellationToken cancellationToken) =>
        publicIdQueryService.FetchProposalPublicIdsAsync(proposal is null ? [] : [proposal], cancellationToken);

    [HttpPost("{matchProposalId:guid}/reject")]
    [EndpointSummary("Reject match proposal")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectMatchProposal(Guid matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await matchProposalCommandService.Handle(
            new RejectMatchProposalCommand(userId, await FindIdAsync(matchProposalId, cancellationToken)),
            cancellationToken);
        var publicIds = await FetchPublicIds(result.IsSuccess ? result.Value : null, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            rejected => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(rejected, publicIds)));
    }

    [HttpPost("{matchProposalId:guid}/cancel")]
    [EndpointSummary("Cancel match proposal")]
    [EndpointDescription("The carrier withdraws its offer. If it was accepted, the request and the route open again.")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelMatchProposal(Guid matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await matchProposalCommandService.Handle(
            new CancelMatchProposalCommand(userId, await FindIdAsync(matchProposalId, cancellationToken)),
            cancellationToken);
        var publicIds = await FetchPublicIds(result.IsSuccess ? result.Value : null, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            cancelled => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(cancelled, publicIds)));
    }
}
