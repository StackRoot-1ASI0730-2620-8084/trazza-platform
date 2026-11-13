namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

public interface IEntityId<out T> where T : notnull
{
    T Value { get; }
}
