using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Domain.Model;

namespace Trazza.Platform.Shared.Interfaces.Rest.Transform;

public static class ProblemDetailsFromCommandErrorAssembler
{
    public static ObjectResult ToActionResultFromError(CommandError error, IStringLocalizer<SharedResource> localizer)
    {
        var (status, titleKey) = error.Kind switch
        {
            CommandErrorKind.Validation => (StatusCodes.Status400BadRequest, "ErrorTitleValidation"),
            CommandErrorKind.Unauthorized => (StatusCodes.Status401Unauthorized, "ErrorTitleUnauthorized"),
            CommandErrorKind.Forbidden => (StatusCodes.Status403Forbidden, "ErrorTitleForbidden"),
            CommandErrorKind.NotFound => (StatusCodes.Status404NotFound, "ErrorTitleNotFound"),
            CommandErrorKind.Conflict => (StatusCodes.Status409Conflict, "ErrorTitleConflict"),
            _ => (StatusCodes.Status500InternalServerError, "ErrorTitleUnexpected")
        };

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = localizer[titleKey],
            Detail = localizer[error.Code],
            Extensions = { ["code"] = error.Code }
        };
        return new ObjectResult(problemDetails)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }
}
