using Trazza.Platform.Iam.Application.Internal.OutboundServices;
using Trazza.Platform.Iam.Domain.Repositories;

namespace Trazza.Platform.Iam.Infrastructure.Pipeline;

public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenService tokenService, IUserRepository userRepository)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var publicId = tokenService.ValidateAccessToken(header["Bearer ".Length..].Trim());
            if (publicId.HasValue)
            {
                var user = await userRepository.FindByPublicIdAsync(publicId.Value, context.RequestAborted);
                if (user is { CanSignIn: true })
                    context.Items[HttpContextAuthenticationExtensions.ItemKey] = new AuthenticatedUser(
                        user.Id, user.PublicId, user.Roles.Select(role => role.Role).ToList());
            }
        }

        await next(context);
    }
}

public static class RequestAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestAuthorization(this IApplicationBuilder builder) =>
        builder.UseMiddleware<RequestAuthorizationMiddleware>();
}
