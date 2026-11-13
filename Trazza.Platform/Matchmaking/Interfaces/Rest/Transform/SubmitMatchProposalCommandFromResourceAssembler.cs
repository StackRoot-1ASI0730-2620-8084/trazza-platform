using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Interfaces.Rest.Transform;

namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Transform;

public static class SubmitMatchProposalCommandFromResourceAssembler
{
    public static SubmitMatchProposalCommand ToCommandFromResource(UserId userId,
        ShipmentRequestId shipmentRequestId, ReturnRouteId returnRouteId, SubmitMatchProposalResource resource)
    {
        return new SubmitMatchProposalCommand(
            userId,
            shipmentRequestId,
            returnRouteId,
            MoneyResourceAssembler.ToValueObjectFromResource(resource.OfferedRate));
    }
}
