namespace TrazzaPlatform.API.Reputation.Interfaces.Acl;

/// <summary>
///     Anti-corruption layer used by Execution to award loyalty points. Changes are saved by the caller.
/// </summary>
public interface IReputationContextFacade
{
    /// <summary>Points for a confirmed trip, once per user and trip.</summary>
    Task AwardTripCompletedAsync(ulong userId, ulong tripId, DateTime now, CancellationToken ct = default);
}