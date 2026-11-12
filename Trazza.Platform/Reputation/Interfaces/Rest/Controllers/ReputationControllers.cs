using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Iam.Infrastructure.Pipeline;
using Trazza.Platform.Reputation.Domain.Model.Commands;
using Trazza.Platform.Reputation.Domain.Services;
using Trazza.Platform.Reputation.Interfaces.Rest.Resources;
using Trazza.Platform.Reputation.Interfaces.Rest.Transform;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Interfaces.Rest;

namespace Trazza.Platform.Reputation.Interfaces.Rest.Controllers;

/// <summary>Ratings between carrier and merchant after a confirmed trip.</summary>
[Route("api/v1/[controller]")]
[Authorize]
public class RatingsController(
    IReputationCommandService commandService,
    IReputationQueryService queryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;

    [HttpPost]
    public async Task<IActionResult> Rate([FromBody] RateTripResource resource, CancellationToken ct) =>
        CreatedFromResult(await commandService.Handle(
                new RateTripCommand(UserId, resource.TripId, resource.Score, resource.Comment), ct),
            ReputationResourceAssembler.ToResource);

    [HttpGet("received")]
    public async Task<IActionResult> Received(CancellationToken ct) =>
        Ok((await queryService.ListReceivedAsync(UserId, ct)).Select(ReputationResourceAssembler.ToResource));

    [HttpGet("given")]
    public async Task<IActionResult> Given(CancellationToken ct) =>
        Ok((await queryService.ListGivenAsync(UserId, ct)).Select(ReputationResourceAssembler.ToResource));

    /// <summary>Average score of any user, by public id. Shown on offers and trips.</summary>
    [HttpGet("summary/{userId:guid}")]
    public async Task<IActionResult> Summary(Guid userId, CancellationToken ct) =>
        FromResult(await queryService.GetSummaryAsync(userId, ct), ReputationResourceAssembler.ToResource);
}

/// <summary>Loyalty points: +10 per confirmed trip, +2 per rating given.</summary>
[Route("api/v1/[controller]")]
[Authorize]
public class LoyaltyController(
    IReputationCommandService commandService,
    IReputationQueryService queryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;

    [HttpGet("points")]
    public async Task<IActionResult> Points(CancellationToken ct) =>
        Ok(ReputationResourceAssembler.ToResource(await queryService.GetLoyaltyAccountAsync(UserId, ct)));

    [HttpPost("redemptions")]
    public async Task<IActionResult> Redeem([FromBody] RedeemPointsResource resource, CancellationToken ct) =>
        CreatedFromResult(await commandService.Handle(new RedeemPointsCommand(UserId, resource.Points), ct),
            ReputationResourceAssembler.ToResource);
}