using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using TrazzaPlatform.API.Iam.Domain.Model.Commands;
using TrazzaPlatform.API.Iam.Domain.Services;
using TrazzaPlatform.API.Iam.Infrastructure.Pipeline;
using TrazzaPlatform.API.Iam.Interfaces.Rest.Resources;
using TrazzaPlatform.API.Iam.Interfaces.Rest.Transform;
using TrazzaPlatform.API.Resources;
using TrazzaPlatform.API.Shared.Interfaces.Rest;

namespace TrazzaPlatform.API.Iam.Interfaces.Rest.Controllers;

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
