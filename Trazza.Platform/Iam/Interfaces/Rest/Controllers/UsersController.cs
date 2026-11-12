using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TrazzaPlatform.API.Iam.Domain.Model.Commands;
using TrazzaPlatform.API.Iam.Domain.Services;
using TrazzaPlatform.API.Iam.Infrastructure.Pipeline;
using TrazzaPlatform.API.Iam.Interfaces.Rest.Resources;
using TrazzaPlatform.API.Iam.Interfaces.Rest.Transform;
using TrazzaPlatform.API.Resources;
using TrazzaPlatform.API.Shared.Domain.Model;
using TrazzaPlatform.API.Shared.Interfaces.Rest;

namespace TrazzaPlatform.API.Iam.Interfaces.Rest.Controllers;

/// <summary>
///     The signed-in account: identity, roles, merchant profile and KYC (simulated RENIEC).
/// </summary>
[Route("api/v1/[controller]/me")]
[Authorize]
public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    IStringLocalizer<SharedResource> localizer) : TrazzaControllerBase(localizer)
{
    private ulong UserId => HttpContext.GetAuthenticatedUser().Id;

    [HttpGet]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var account = await userQueryService.GetAccountAsync(UserId, ct);
        return account is null ? ErrorResult(CommandError.NotFound("user-not-found")) : Ok(IamResourceAssembler.ToResource(account));
    }

    [HttpPut("identity")]
    public async Task<IActionResult> UpdateIdentity([FromBody] UpdateIdentityResource resource, CancellationToken ct) =>
        FromResult(await userCommandService.Handle(new UpdateIdentityCommand(UserId, resource.FullName, resource.Phone), ct),
            IamResourceAssembler.ToResource);

    [HttpPost("roles")]
    public async Task<IActionResult> AddRole([FromBody] AddRoleResource resource, CancellationToken ct)
    {
        var command = IamResourceAssembler.ToCommand(UserId, resource);
        if (command.IsFailure) return ErrorResult(command.Error!);
        return FromResult(await userCommandService.Handle(command.Value!, ct), IamResourceAssembler.ToResource);
    }

    [HttpPut("merchant-profile")]
    public async Task<IActionResult> UpdateMerchantProfile([FromBody] UpdateMerchantProfileResource resource,
        CancellationToken ct) =>
        FromResult(await userCommandService.Handle(
                new UpdateMerchantProfileCommand(UserId, resource.BusinessName, resource.BusinessType), ct),
            IamResourceAssembler.ToResource);

    [HttpGet("identity-verifications")]
    public async Task<IActionResult> ListIdentityVerifications(CancellationToken ct) =>
        Ok((await userQueryService.ListVerificationsAsync(UserId, ct)).Select(IamResourceAssembler.ToResource));

    [HttpPost("identity-verifications")]
    public async Task<IActionResult> SubmitIdentityVerification([FromBody] SubmitIdentityVerificationResource resource,
        CancellationToken ct) =>
        CreatedFromResult(await userCommandService.Handle(
                new SubmitIdentityVerificationCommand(UserId, resource.DocumentFrontS3Key, resource.DocumentBackS3Key), ct),
            IamResourceAssembler.ToResource);
}
