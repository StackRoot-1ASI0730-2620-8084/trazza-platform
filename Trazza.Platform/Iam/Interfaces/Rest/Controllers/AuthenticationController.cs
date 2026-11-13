using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Iam.Domain.Model.Commands;
using Trazza.Platform.Iam.Domain.Services;
using Trazza.Platform.Iam.Infrastructure.Pipeline;
using Trazza.Platform.Iam.Interfaces.Rest.Resources;
using Trazza.Platform.Iam.Interfaces.Rest.Transform;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Interfaces.Rest;

namespace Trazza.Platform.Iam.Interfaces.Rest.Controllers;

/// <summary>
///     Own authentication: sign-up, sign-in, refresh token rotation and sign-out.
/// </summary>
[Route("api/v1/[controller]")]
[Authorize]
public class AuthenticationController(IUserCommandService userCommandService, IStringLocalizer<SharedResource> localizer)
    : TrazzaControllerBase(localizer)
{
    [HttpPost("sign-up")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource, CancellationToken ct)
    {
        var command = IamResourceAssembler.ToCommand(resource);
        if (command.IsFailure) return ErrorResult(command.Error!);
        return CreatedFromResult(await userCommandService.Handle(command.Value!, ct), IamResourceAssembler.ToResource);
    }

    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource, CancellationToken ct) =>
        FromResult(await userCommandService.Handle(new SignInCommand(resource.Email, resource.Password), ct),
            IamResourceAssembler.ToResource);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshSessionResource resource, CancellationToken ct) =>
        FromResult(await userCommandService.Handle(new RefreshSessionCommand(resource.RefreshToken), ct),
            IamResourceAssembler.ToResource);

    [HttpPost("sign-out")]
    public async Task<IActionResult> SignOut([FromBody] RefreshSessionResource resource, CancellationToken ct) =>
        NoContentFromResult(await userCommandService.Handle(
            new SignOutCommand(HttpContext.GetAuthenticatedUser().Id, resource.RefreshToken), ct));
}
