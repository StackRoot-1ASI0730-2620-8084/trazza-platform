using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public readonly record struct UserId(ulong Value) : IEntityId<ulong>
{
    public static implicit operator ulong(UserId id) => id.Value;
    public static implicit operator UserId(ulong value) => new(value);

    public override string ToString() => Value.ToString();
}
