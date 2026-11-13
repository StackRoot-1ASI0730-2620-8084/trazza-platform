namespace Trazza.Platform.Shared.Domain.Model.Errors;

public sealed record Error(string Code, string Message, EErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, EErrorType.None);

    public bool IsNone => Type == EErrorType.None;

    public static Error Failure(string code, string message) => new(code, message, EErrorType.Failure);

    public static Error Validation(string code, string message) => new(code, message, EErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, EErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, EErrorType.Conflict);

    public static Error Unauthorized(string code, string message) => new(code, message, EErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) => new(code, message, EErrorType.Forbidden);

    public static Error Unexpected(string code, string message) => new(code, message, EErrorType.Unexpected);
}