using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public readonly record struct DriverId(ulong Value) : IEntityId<ulong>
{
    public static implicit operator ulong(DriverId id) => id.Value;
    public static implicit operator DriverId(ulong value) => new(value);

    public override string ToString() => Value.ToString();
}
