using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class MatchProposalResourceFromEntityAssembler
{
    public static IEnumerable<MatchProposalResource> ToResourcesFromEntities(IEnumerable<MatchProposal> proposals,
        ProposalPublicIds publicIds, IReadOnlyDictionary<ulong, Guid> tripIds) =>
        proposals.Select(proposal => ToResourceFromEntity(proposal, publicIds,
            tripIds.TryGetValue(proposal.Id, out var tripId) ? tripId : null));

    /// <summary>
    ///     The route and the shipment request are referenced by their public ids. The trip id is the public id of
    ///     the trip that Execution created when the proposal was accepted.
    /// </summary>
    public static MatchProposalResource ToResourceFromEntity(MatchProposal proposal, ProposalPublicIds publicIds,
        Guid? tripId = null)
    {
        return new MatchProposalResource(
            proposal.PublicId,
            publicIds.ShipmentRequests.GetValueOrDefault(proposal.ShipmentRequestId),
            publicIds.ReturnRoutes.GetValueOrDefault(proposal.ReturnRouteId),
            MoneyResourceAssembler.ToResourceFromValueObject(proposal.OfferedRate),
            proposal.Status.ToSnakeCaseValue(),
            proposal.RespondedAt,
            proposal.CreatedAt,
            tripId);
    }
}
