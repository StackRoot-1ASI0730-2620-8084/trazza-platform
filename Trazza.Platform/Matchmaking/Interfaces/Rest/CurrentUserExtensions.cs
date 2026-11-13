using Microsoft.AspNetCore.Mvc;
using Trazza.Platform.Iam.Infrastructure.Pipeline;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest;

public static class CurrentUserExtensions
{
    public static UserId? GetCurrentUserId(this ControllerBase controller) =>
        controller.HttpContext.FindAuthenticatedUser() is { Id: > 0 } user ? new UserId(user.Id) : null;
}
