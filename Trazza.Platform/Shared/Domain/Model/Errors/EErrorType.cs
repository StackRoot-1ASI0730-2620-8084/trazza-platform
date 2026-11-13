namespace Trazza.Platform.Shared.Domain.Model.Errors;


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