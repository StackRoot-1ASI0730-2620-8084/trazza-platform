using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Queries;

/// <summary>
///     Proposals on the routes of the user as carrier and on the shipment requests of the user as merchant.
/// </summary>
public record GetProposalsByUserQuery(UserId UserId);
