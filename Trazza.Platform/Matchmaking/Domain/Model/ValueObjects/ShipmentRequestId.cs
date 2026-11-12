using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public readonly record struct ShipmentRequestId(ulong Value) : IEntityId<ulong>
{
    public static implicit operator ulong(ShipmentRequestId id) => id.Value;
    public static implicit operator ShipmentRequestId(ulong value) => new(value);

    public override string ToString() => Value.ToString();
}
