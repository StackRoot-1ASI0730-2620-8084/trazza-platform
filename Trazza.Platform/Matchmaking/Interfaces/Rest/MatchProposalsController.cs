using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Trazza.Platform.Execution.Interfaces.Acl;
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
[Tags("Match proposals")]
public class MatchProposalsController(
    IMatchProposalCommandService matchProposalCommandService,
    IMatchProposalQueryService matchProposalQueryService,
    IExecutionContextFacade executionContextFacade,
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

        var command = SubmitMatchProposalCommandFromResourceAssembler.ToCommandFromResource(userId, resource);
        var result = await matchProposalCommandService.Handle(command, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            submitted => CreatedAtAction(nameof(GetMatchProposalById), new { matchProposalId = submitted.Id.Value },
                MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(submitted)));
    }

    [HttpGet("{matchProposalId}")]
    [EndpointSummary("Get match proposal by id")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMatchProposalById(ulong matchProposalId, CancellationToken cancellationToken)
    {
        var proposal = await matchProposalQueryService.Handle(
            new GetMatchProposalByIdQuery(new MatchProposalId(matchProposalId)), cancellationToken);
        var tripId = await FindTripIdAsync(matchProposalId, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromEntity(
            this,
            proposal,
            MatchmakingErrors.MatchProposalNotFound,
            problemDetailsFactory,
            found => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(found, tripId)));
    }

    [HttpPost("{matchProposalId}/accept")]
    [EndpointSummary("Accept match proposal")]
    [EndpointDescription(
        "The merchant accepts the offer: the shipment request is matched and its other pending proposals expire.")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcceptMatchProposal(ulong matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await matchProposalCommandService.Handle(
            new AcceptMatchProposalCommand(userId, new MatchProposalId(matchProposalId)), cancellationToken);
        var tripId = result.IsSuccess ? await FindTripIdAsync(matchProposalId, cancellationToken) : null;

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            accepted => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(accepted, tripId)));
    }

    private async Task<Guid?> FindTripIdAsync(ulong matchProposalId, CancellationToken cancellationToken) =>
        (await executionContextFacade.FindTripPublicIdsByProposalIdsAsync([matchProposalId], cancellationToken))
        .TryGetValue(matchProposalId, out var tripId)
            ? tripId
            : null;

    [HttpPost("{matchProposalId}/reject")]
    [EndpointSummary("Reject match proposal")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RejectMatchProposal(ulong matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await matchProposalCommandService.Handle(
            new RejectMatchProposalCommand(userId, new MatchProposalId(matchProposalId)), cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            rejected => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(rejected)));
    }

    [HttpPost("{matchProposalId}/cancel")]
    [EndpointSummary("Cancel match proposal")]
    [EndpointDescription("The carrier withdraws its offer. If it was accepted, the request and the route open again.")]
    [ProducesResponseType(typeof(MatchProposalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelMatchProposal(ulong matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await matchProposalCommandService.Handle(
            new CancelMatchProposalCommand(userId, new MatchProposalId(matchProposalId)), cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            cancelled => Ok(MatchProposalResourceFromEntityAssembler.ToResourceFromEntity(cancelled)));
    }
}
