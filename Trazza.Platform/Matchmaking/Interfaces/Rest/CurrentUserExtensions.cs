using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest;

/// <summary>
///     Gets the id of the user who makes the request. It reads the id claim of the token.
///     Until the IAM context is merged, in Development it also accepts the X-User-Id header to test the API.
///     Replace it with the authenticated user of IAM when that context is merged.
/// </summary>
public static class CurrentUserExtensions
{
    public const string DevelopmentUserHeader = "X-User-Id";

    public static UserId? GetCurrentUserId(this ControllerBase controller)
    {
        var value = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                    controller.User.FindFirst("sub")?.Value;

        if (value is null && controller.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>()
                .IsDevelopment())
            value = controller.Request.Headers[DevelopmentUserHeader].FirstOrDefault();

        return ulong.TryParse(value, out var id) && id > 0 ? new UserId(id) : null;
    }
}
