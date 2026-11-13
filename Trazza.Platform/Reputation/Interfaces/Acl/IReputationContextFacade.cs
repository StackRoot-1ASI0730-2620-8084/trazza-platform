namespace Trazza.Platform.Reputation.Interfaces.Acl;

public interface IReputationContextFacade
{
    Task AwardTripCompletedAsync(ulong userId, ulong tripId, DateTime now, CancellationToken ct = default);
}