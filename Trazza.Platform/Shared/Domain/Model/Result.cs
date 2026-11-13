namespace Trazza.Platform.Shared.Domain.Model;

/// <summary>
///     Outcome of a domain or application operation: either a value or an error, never both.
/// </summary>
/// <typeparam name="TValue">Type of the value returned on success.</typeparam>
/// <typeparam name="TError">Type of the error returned on failure.</typeparam>
public sealed class Result<TValue, TError>
{
    private Result(TValue? value, TError? error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }

    public TValue? Value { get; }
    public TError? Error { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public static Result<TValue, TError> Success(TValue value) => new(value, default, true);

    public static Result<TValue, TError> Failure(TError error) => new(default, error, false);

    public static implicit operator Result<TValue, TError>(TValue value) => Success(value);

    public static implicit operator Result<TValue, TError>(TError error) => Failure(error);

    /// <summary>
    ///     Runs <paramref name="onSuccess" /> or <paramref name="onFailure" /> depending on the outcome.
    /// </summary>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}