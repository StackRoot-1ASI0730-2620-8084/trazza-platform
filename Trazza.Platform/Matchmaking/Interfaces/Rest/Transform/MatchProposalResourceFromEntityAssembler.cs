using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class MatchProposalResourceFromEntityAssembler
{
    public static IEnumerable<MatchProposalResource> ToResourcesFromEntities(IEnumerable<MatchProposal> proposals,
        IReadOnlyDictionary<ulong, Guid> tripIds) =>
        proposals.Select(proposal =>
            ToResourceFromEntity(proposal, tripIds.TryGetValue(proposal.Id, out var tripId) ? tripId : null));

    public static MatchProposalResource ToResourceFromEntity(MatchProposal proposal) =>
        ToResourceFromEntity(proposal, null);

    /// <summary>
    ///     The trip id is the public id of the trip that Execution created when the proposal was accepted.
    /// </summary>
    public static MatchProposalResource ToResourceFromEntity(MatchProposal proposal, Guid? tripId)
    {
        return new MatchProposalResource(
            proposal.Id,
            proposal.ShipmentRequestId,
            proposal.ReturnRouteId,
            MoneyResourceAssembler.ToResourceFromValueObject(proposal.OfferedRate),
            proposal.Status.ToSnakeCaseValue(),
            proposal.RespondedAt,
            proposal.CreatedAt,
            tripId);
    }
}
