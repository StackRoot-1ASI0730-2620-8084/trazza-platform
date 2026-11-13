namespace Trazza.Platform.Shared.Domain.Model;

public enum CommandErrorKind
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict
}

public sealed record CommandError(CommandErrorKind Kind, string Code)
{
    public static CommandError Validation(string code) => new(CommandErrorKind.Validation, code);

    public static CommandError Unauthorized(string code) => new(CommandErrorKind.Unauthorized, code);

    public static CommandError Forbidden(string code) => new(CommandErrorKind.Forbidden, code);

    public static CommandError NotFound(string code) => new(CommandErrorKind.NotFound, code);

    public static CommandError Conflict(string code) => new(CommandErrorKind.Conflict, code);
}