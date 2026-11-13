using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Iam.Infrastructure.Pipeline;

/// <summary>
///     Requires an authenticated user and, optionally, one of the given roles (<c>CARRIER</c>, <c>MERCHANT</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AuthorizeAttribute(params string[] roles) : Attribute, IAuthorizationFilter
{
    public IReadOnlyCollection<string> Roles { get; } = roles;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()) return;

        var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();
        var user = context.HttpContext.FindAuthenticatedUser();
        if (user is null)
        {
            context.Result = ProblemDetailsFromCommandErrorAssembler.ToActionResultFromError(
                CommandError.Unauthorized("authentication-required"), localizer);
            return;
        }

        if (Roles.Count == 0) return;
        var userRoles = user.Roles.Select(role => EnumText.ToText(role)).ToHashSet();
        if (!Roles.Any(userRoles.Contains))
            context.Result = ProblemDetailsFromCommandErrorAssembler.ToActionResultFromError(
                CommandError.Forbidden("role-required"), localizer);
    }
}

/// <summary>
///     Marks an action that does not need a bearer token (sign-up, sign-in and refresh).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AllowAnonymousAttribute : Attribute
{
}
