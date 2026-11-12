using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class MatchProposalResourceFromEntityAssembler
{
    public static MatchProposalResource ToResourceFromEntity(MatchProposal proposal)
    {
        return new MatchProposalResource(
            proposal.Id,
            proposal.ShipmentRequestId,
            proposal.ReturnRouteId,
            MoneyResourceAssembler.ToResourceFromValueObject(proposal.OfferedRate),
            proposal.Status.ToSnakeCaseValue(),
            proposal.RespondedAt,
            proposal.CreatedAt);
    }
}
