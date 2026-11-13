using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Shared.Domain.Model.Errors;
using Trazza.Platform.Shared.Resources.Errors;

namespace Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails;

public class ProblemDetailsFactory(IStringLocalizer<ErrorMessages> localizer)
{
    public IActionResult CreateProblemDetails(ControllerBase controller, Error error)
    {
        var status = ToStatusCode(error.Type);
        var title = localizer[error.Code];

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = status,
            Title = title.ResourceNotFound ? error.Message : title.Value,
            Instance = controller.HttpContext.Request.Path
        };
        problemDetails.Extensions["code"] = error.Code;

        return new ObjectResult(problemDetails)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static int ToStatusCode(EErrorType type)
    {
        return type switch
        {
            EErrorType.Validation => StatusCodes.Status400BadRequest,
            EErrorType.NotFound => StatusCodes.Status404NotFound,
            EErrorType.Conflict => StatusCodes.Status409Conflict,
            EErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            EErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
