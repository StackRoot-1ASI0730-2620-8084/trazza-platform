using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Interfaces.Acl;

namespace Trazza.Platform.Matchmaking.Application.Acl;

public class MatchmakingContextFacade(
    IMatchProposalQueryService matchProposalQueryService,
    IReturnRouteQueryService returnRouteQueryService,
    IShipmentRequestQueryService shipmentRequestQueryService)
    : IMatchmakingContextFacade
{
    public async Task<AcceptedMatchSummary?> FetchAcceptedMatch(ulong matchProposalId,
        CancellationToken cancellationToken)
    {
        var proposal = await matchProposalQueryService.Handle(new GetMatchProposalByIdQuery(matchProposalId),
            cancellationToken);
        if (proposal is null || !proposal.IsAccepted) return null;

        var route = await returnRouteQueryService.Handle(new GetReturnRouteByIdQuery(proposal.ReturnRouteId),
            cancellationToken);
        var shipment = await shipmentRequestQueryService.Handle(
            new GetShipmentRequestByIdQuery(proposal.ShipmentRequestId), cancellationToken);
        if (route is null || shipment is null) return null;

        return new AcceptedMatchSummary(proposal.Id, shipment.Id, route.Id, route.CarrierProfileId,
            shipment.MerchantProfileId, route.VehicleId, route.DriverId?.Value, proposal.OfferedRate.Amount,
            proposal.OfferedRate.Currency);
    }
}
