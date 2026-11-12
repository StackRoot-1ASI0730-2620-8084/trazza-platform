using System.Net.Mime;
using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Billing.Interfaces.Rest.Transform;
using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Trazza.Platform.Billing.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/users/{userId:int}/subscription")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscriptions")]
public class SubscriptionsController(IPaymentTransactionQueryService paymentTransactionQueryService)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get user subscription")]
    [EndpointDescription("Gets the current plan of a user; Free when there is no paid period covering today.")]
    [ProducesResponseType(typeof(SubscriptionResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserSubscription(int userId, CancellationToken cancellationToken)
    {
        var activeSubscription = await paymentTransactionQueryService.Handle(
            new GetActiveSubscriptionsByUserIdQuery(new UserId(userId)), cancellationToken);
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromActiveSubscription(userId, activeSubscription));
    }
}
