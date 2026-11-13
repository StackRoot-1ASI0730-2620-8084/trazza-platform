using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Billing.Domain.Model.Commands;
using Trazza.Platform.Billing.Domain.Services;
using Trazza.Platform.Billing.Interfaces.Rest.Resources;
using Trazza.Platform.Billing.Interfaces.Rest.Transform;
using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Iam.Infrastructure.Pipeline;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Interfaces.Rest;
 
namespace Trazza.Platform.Billing.Interfaces.Rest.Controllers;
 
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class PlansController(IBillingQueryService queryService, IStringLocalizer<SharedResource> localizer)
    : TrazzaControllerBase(localizer)
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok((await queryService.ListPlansAsync(ct)).Select(BillingResourceAssembler.ToResource));
}
 
[Route("api/v1/[controller]")]
[Authorize(Roles.Carrier)]
public class SubscriptionsController(
    ISubscriptionCommandService commandService,
    IBillingQueryService queryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;
 
    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken ct) =>
        Ok((await queryService.ListMySubscriptionsAsync(UserId, ct)).Select(BillingResourceAssembler.ToResource));
 
    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeResource resource, CancellationToken ct) =>
        CreatedFromResult(await commandService.Handle(new SubscribeCommand(UserId, resource.PlanCode,
                resource.VehicleIds ?? []), ct),
            BillingResourceAssembler.ToResource);
 
    [HttpPost("{subscriptionId}/renew")]
    public async Task<IActionResult> Renew(ulong subscriptionId, CancellationToken ct) =>
        FromResult(await commandService.Handle(new RenewSubscriptionCommand(UserId, subscriptionId), ct),
            BillingResourceAssembler.ToResource);
 
    [HttpPost("{subscriptionId}/cancel")]
    public async Task<IActionResult> Cancel(ulong subscriptionId, CancellationToken ct) =>
        FromResult(await commandService.Handle(new CancelSubscriptionCommand(UserId, subscriptionId), ct),
            BillingResourceAssembler.ToResource);
}
 
[Route("api/v1/[controller]")]
[Authorize]
public class FreightSettlementsController(
    ISettlementCommandService commandService,
    IBillingQueryService queryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;
 
    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken ct) =>
        Ok((await queryService.ListMySettlementsAsync(UserId, ct)).Select(BillingResourceAssembler.ToResource));
 
    [HttpPost("{tripId:guid}/mark-paid")]
    [Authorize(Roles.Merchant)]
    public async Task<IActionResult> MarkPaid(Guid tripId, [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] MarkFreightPaidResource? resource,
        CancellationToken ct) =>
        FromResult(await commandService.Handle(new MarkFreightPaidCommand(UserId, tripId, resource?.ExternalReference), ct),
            BillingResourceAssembler.ToResource);
 
    [HttpPost("{tripId:guid}/confirm")]
    [Authorize(Roles.Carrier)]
    public async Task<IActionResult> Confirm(Guid tripId, CancellationToken ct) =>
        FromResult(await commandService.Handle(new ConfirmFreightReceivedCommand(UserId, tripId), ct),
            BillingResourceAssembler.ToResource);
}
 
[Route("api/v1/[controller]")]
[Authorize]
public class CancellationCompensationsController(
    ISettlementCommandService commandService,
    IBillingQueryService queryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;
 
    [HttpGet]
    public async Task<IActionResult> ListMine(CancellationToken ct) =>
        Ok((await queryService.ListMyCompensationsAsync(UserId, ct)).Select(BillingResourceAssembler.ToResource));
 
    [HttpPost("{tripId:guid}/confirm-paid")]
    [Authorize(Roles.Carrier)]
    public Task<IActionResult> ConfirmPaid(Guid tripId, CancellationToken ct) =>
        UpdateAsync(tripId, CompensationAction.ConfirmPaid, ct);
 
    [HttpPost("{tripId:guid}/waive")]
    [Authorize(Roles.Carrier)]
    public Task<IActionResult> Waive(Guid tripId, CancellationToken ct) => UpdateAsync(tripId, CompensationAction.Waive, ct);
 
    [HttpPost("{tripId:guid}/dispute")]
    [Authorize(Roles.Merchant)]
    public Task<IActionResult> Dispute(Guid tripId, CancellationToken ct) =>
        UpdateAsync(tripId, CompensationAction.Dispute, ct);
 
    private async Task<IActionResult> UpdateAsync(Guid tripId, CompensationAction action, CancellationToken ct) =>
        FromResult(await commandService.Handle(new UpdateCompensationCommand(UserId, tripId, action), ct),
            BillingResourceAssembler.ToResource);
}
