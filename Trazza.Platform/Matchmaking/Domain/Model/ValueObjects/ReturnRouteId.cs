using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public readonly record struct ReturnRouteId(ulong Value) : IEntityId<ulong>
{
    public static implicit operator ulong(ReturnRouteId id) => id.Value;
    public static implicit operator ReturnRouteId(ulong value) => new(value);

    public override string ToString() => Value.ToString();
}
