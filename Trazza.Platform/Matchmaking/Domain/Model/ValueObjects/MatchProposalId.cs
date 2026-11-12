using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

public readonly record struct MatchProposalId(ulong Value) : IEntityId<ulong>
{
    public static implicit operator ulong(MatchProposalId id) => id.Value;
    public static implicit operator MatchProposalId(ulong value) => new(value);

    public override string ToString() => Value.ToString();
}
