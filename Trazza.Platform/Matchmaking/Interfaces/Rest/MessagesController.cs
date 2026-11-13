using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Trazza.Platform.Matchmaking.Application.CommandServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;
using Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest;

[ApiController]
[Route("api/v1/match-proposals/{matchProposalId:guid}/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Messages")]
public class MessagesController(
    IMessageCommandService messageCommandService,
    IMessageQueryService messageQueryService,
    IPublicIdQueryService publicIdQueryService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get messages of a proposal")]
    [EndpointDescription("Only the carrier of the route and the merchant of the request can read the chat.")]
    [ProducesResponseType(typeof(IEnumerable<MessageResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMessages(Guid matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await messageQueryService.Handle(
            new GetMessagesByProposalQuery(userId, await FindIdAsync(matchProposalId, cancellationToken)), cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            messages => Ok(messages.Select(message =>
                MessageResourceFromEntityAssembler.ToResourceFromEntity(message, matchProposalId))));
    }

    [HttpPost]
    [EndpointSummary("Send message")]
    [ProducesResponseType(typeof(MessageResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendMessage(Guid matchProposalId, [FromBody] SendMessageResource resource,
        CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var command = SendMessageCommandFromResourceAssembler.ToCommandFromResource(userId,
            await FindIdAsync(matchProposalId, cancellationToken), resource);
        var result = await messageCommandService.Handle(command, cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            sent => CreatedAtAction(nameof(GetMessages), new { matchProposalId },
                MessageResourceFromEntityAssembler.ToResourceFromEntity(sent, matchProposalId)));
    }

    [HttpPost("read")]
    [EndpointSummary("Mark messages as read")]
    [EndpointDescription("Marks as read the messages sent by the other participant and returns how many were marked.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkMessagesAsRead(Guid matchProposalId, CancellationToken cancellationToken)
    {
        if (this.GetCurrentUserId() is not { } userId) return Unauthorized();

        var result = await messageCommandService.Handle(
            new MarkMessagesAsReadCommand(userId, await FindIdAsync(matchProposalId, cancellationToken)), cancellationToken);

        return MatchmakingActionResultAssembler.ToActionResultFromResult(
            this,
            result,
            problemDetailsFactory,
            marked => Ok(new { marked }));
    }

    private async Task<MatchProposalId> FindIdAsync(Guid publicId, CancellationToken cancellationToken) =>
        await publicIdQueryService.FindMatchProposalIdAsync(publicId, cancellationToken) ?? new MatchProposalId(0);
}
