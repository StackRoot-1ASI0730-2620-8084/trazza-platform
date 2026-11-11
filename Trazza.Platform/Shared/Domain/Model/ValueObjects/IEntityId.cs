namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

/// <summary>
///     Contract for strongly-typed identifiers that wrap a primitive value.
/// </summary>
/// <typeparam name="T">The primitive type of the identifier value.</typeparam>
public interface IEntityId<out T> where T : notnull
{
    T Value { get; }
}
