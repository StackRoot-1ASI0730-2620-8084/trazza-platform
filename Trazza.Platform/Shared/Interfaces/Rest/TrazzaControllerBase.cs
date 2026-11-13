using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Trazza.Platform.Resources;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Shared.Interfaces.Rest;

/// <summary>
///     Base class of every REST controller: turns a <see cref="Result{TValue,TError}" /> into an HTTP response.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class TrazzaControllerBase(IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    protected IStringLocalizer<SharedResource> Localizer { get; } = localizer;

    protected IActionResult ErrorResult(CommandError error) =>
        ProblemDetailsFromCommandErrorAssembler.ToActionResultFromError(error, Localizer);

    protected IActionResult FromResult<TValue, TResource>(Result<TValue, CommandError> result,
        Func<TValue, TResource> toResource) =>
        result.IsSuccess ? Ok(toResource(result.Value!)) : ErrorResult(result.Error!);

    protected IActionResult CreatedFromResult<TValue, TResource>(Result<TValue, CommandError> result,
        Func<TValue, TResource> toResource) =>
        result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, toResource(result.Value!))
            : ErrorResult(result.Error!);

    protected IActionResult NoContentFromResult<TValue>(Result<TValue, CommandError> result) =>
        result.IsSuccess ? NoContent() : ErrorResult(result.Error!);
}
