namespace Trazza.Platform.Shared.Domain.Model.Errors;

/// <summary>
///     Classifies a domain error so it can be translated into the right HTTP status.
/// </summary>

public enum EErrorType
{
    None,
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Unexpected
}