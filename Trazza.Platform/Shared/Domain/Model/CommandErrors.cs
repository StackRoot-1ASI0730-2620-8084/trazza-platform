namespace Trazza.Platform.Shared.Domain.Model;

/// <summary>
///     Category of a business error; the interfaces layer maps it to an HTTP status code.
/// </summary>
public enum CommandErrorKind
{
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict
}

/// <summary>
///     Business error with a stable kebab-case <paramref name="Code" /> that is also the key of its
///     localized message in <c>SharedResource.*.resx</c>.
/// </summary>
/// <param name="Kind">Category of the error.</param>
/// <param name="Code">Stable error code, e.g. <c>email-already-registered</c>.</param>
public sealed record CommandError(CommandErrorKind Kind, string Code)
{
    public static CommandError Validation(string code) => new(CommandErrorKind.Validation, code);

    public static CommandError Unauthorized(string code) => new(CommandErrorKind.Unauthorized, code);

    public static CommandError Forbidden(string code) => new(CommandErrorKind.Forbidden, code);

    public static CommandError NotFound(string code) => new(CommandErrorKind.NotFound, code);

    public static CommandError Conflict(string code) => new(CommandErrorKind.Conflict, code);
}