using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public readonly record struct CarrierProfileId(ulong Value) : IEntityId<ulong>
{
    public static implicit operator ulong(CarrierProfileId id) => id.Value;
    public static implicit operator CarrierProfileId(ulong value) => new(value);

    public override string ToString() => Value.ToString();
}
