using System.Net.Mime;
using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Model.Errors;
using Trazza.Platform.Billing.Domain.Model.Queries;
using Trazza.Platform.Billing.Domain.Model.ValueObjects;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Billing.Interfaces.Rest.Transform;
using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Trazza.Platform.Billing.Interfaces.Rest;

[Authorize]
[ApiController]
[Route("api/v1/subscriptions/{subscriptionId}/payments")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Subscriptions")]
public class SubscriptionPaymentsController(
    ISubscriptionPaymentQueryService subscriptionPaymentQueryService,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Get subscription payments")]
    [EndpointDescription("Gets the monthly payments of a subscription, newest first.")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPaymentResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPayments(ulong subscriptionId,
        CancellationToken cancellationToken)
    {
        var payments = await subscriptionPaymentQueryService.Handle(
            new GetPaymentsBySubscriptionIdQuery(new SubscriptionId(subscriptionId)), cancellationToken);
        return Ok(payments.Select(SubscriptionPaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{paymentId}")]
    [EndpointSummary("Get subscription payment by id")]
    [EndpointDescription("Gets one payment of a subscription by its identifier.")]
    [ProducesResponseType(typeof(SubscriptionPaymentResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionPaymentById(ulong subscriptionId, ulong paymentId,
        CancellationToken cancellationToken)
    {
        var payment = await subscriptionPaymentQueryService.Handle(
            new GetSubscriptionPaymentByIdQuery(new SubscriptionPaymentId(paymentId)), cancellationToken);
        if (payment is not null && payment.SubscriptionId.Value != subscriptionId) payment = null;

        return BillingActionResultAssembler.ToActionResultFromEntity(
            this,
            payment,
            BillingErrors.SubscriptionPaymentNotFound,
            problemDetailsFactory,
            found => Ok(SubscriptionPaymentResourceFromEntityAssembler.ToResourceFromEntity(found)));
    }
}
