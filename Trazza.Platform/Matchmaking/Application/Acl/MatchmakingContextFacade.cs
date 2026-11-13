using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Matchmaking.Interfaces.Acl;

namespace Trazza.Platform.Matchmaking.Application.Acl;

public class MatchmakingContextFacade(
    IMatchProposalQueryService matchProposalQueryService,
    IReturnRouteQueryService returnRouteQueryService,
    IShipmentRequestQueryService shipmentRequestQueryService,
    IMatchProposalRepository matchProposalRepository,
    IReturnRouteRepository returnRouteRepository,
    IShipmentRequestRepository shipmentRequestRepository,
    IMessageRepository messageRepository)
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

    public async Task<List<ulong>> ListProposalIdsWithTripsAsync(ulong? carrierProfileId, ulong? merchantProfileId,
        CancellationToken cancellationToken)
    {
        var proposals = new List<MatchProposal>();
        if (carrierProfileId is { } carrierId)
            foreach (var route in await returnRouteRepository.FindByCarrierProfileIdAsNoTrackingAsync(
                         new CarrierProfileId(carrierId), cancellationToken))
                proposals.AddRange(
                    await matchProposalRepository.FindByReturnRouteIdAsNoTrackingAsync(route.Id, cancellationToken));

        if (merchantProfileId is { } merchantId)
            foreach (var shipment in await shipmentRequestRepository.FindByMerchantProfileIdAsNoTrackingAsync(
                         new MerchantProfileId(merchantId), cancellationToken))
                proposals.AddRange(await matchProposalRepository.FindByShipmentRequestIdAsNoTrackingAsync(shipment.Id,
                    cancellationToken));

        return proposals
            .Where(proposal => proposal.Status is EMatchProposalStatus.Accepted or EMatchProposalStatus.Cancelled)
            .Select(proposal => proposal.Id.Value)
            .Distinct()
            .ToList();
    }

    public async Task<List<TripContextSnapshot>> ListTripContextsAsync(IReadOnlyCollection<ulong> matchProposalIds,
        CancellationToken cancellationToken)
    {
        var contexts = new List<TripContextSnapshot>();
        foreach (var id in matchProposalIds.Distinct())
        {
            var proposal = await matchProposalRepository.FindByIdAsNoTrackingAsync(new MatchProposalId(id),
                cancellationToken);
            if (proposal is null) continue;
            var route = await returnRouteRepository.FindByIdAsNoTrackingAsync(proposal.ReturnRouteId,
                cancellationToken);
            var shipment = await shipmentRequestRepository.FindByIdAsNoTrackingAsync(proposal.ShipmentRequestId,
                cancellationToken);
            if (route is null || shipment is null) continue;

            contexts.Add(new TripContextSnapshot(proposal.Id, proposal.PublicId, route.PublicId, shipment.PublicId,
                route.CarrierProfileId, shipment.MerchantProfileId, route.VehicleId, route.DriverId?.Value,
                route.Origin.Address, ToLocation(route.Origin), route.Destination.Address,
                ToLocation(route.Destination), shipment.Pickup.Address, ToLocation(shipment.Pickup),
                shipment.Delivery.Address, ToLocation(shipment.Delivery), route.DepartureAt.UtcDateTime,
                shipment.Description, shipment.Cargo.WeightKg, shipment.Cargo.VolumeM3));
        }

        return contexts;
    }

    public async Task MarkPickedUpAsync(ulong matchProposalId, CancellationToken cancellationToken)
    {
        var proposal = await matchProposalRepository.FindByIdAsync(new MatchProposalId(matchProposalId),
            cancellationToken);
        if (proposal is null) return;
        (await shipmentRequestRepository.FindByIdAsync(proposal.ShipmentRequestId, cancellationToken))?.MarkInTransit();
        (await returnRouteRepository.FindByIdAsync(proposal.ReturnRouteId, cancellationToken))?.Start();
    }

    public async Task MarkDeliveredAsync(ulong matchProposalId, CancellationToken cancellationToken)
    {
        var proposal = await matchProposalRepository.FindByIdAsync(new MatchProposalId(matchProposalId),
            cancellationToken);
        if (proposal is null) return;
        (await shipmentRequestRepository.FindByIdAsync(proposal.ShipmentRequestId, cancellationToken))?.MarkDelivered();
        await CompleteRouteIfDoneAsync(proposal.ReturnRouteId, cancellationToken);
    }

    public async Task ReleaseAsync(ulong matchProposalId, bool cancelledByMerchant, string reason, DateTime now,
        CancellationToken cancellationToken)
    {
        var proposal = await matchProposalRepository.FindByIdAsync(new MatchProposalId(matchProposalId),
            cancellationToken);
        if (proposal is null) return;
        proposal.Cancel(now);

        var shipment = await shipmentRequestRepository.FindByIdAsync(proposal.ShipmentRequestId, cancellationToken);
        if (shipment is not null)
        {
            if (cancelledByMerchant || shipment.PickupWindow.Until <= now) shipment.Cancel(reason, now);
            else shipment.Reopen();
        }

        var route = await returnRouteRepository.FindByIdAsync(proposal.ReturnRouteId, cancellationToken);
        if (route is null) return;
        if (route.Status == EReturnRouteStatus.Full && route.DepartureAt > now) route.Reopen();
        else await CompleteRouteIfDoneAsync(route.Id, cancellationToken);
    }

    public Task<int> PurgeChatAsync(ulong matchProposalId, CancellationToken cancellationToken) =>
        messageRepository.DeleteByMatchProposalIdAsync(new MatchProposalId(matchProposalId), cancellationToken);

    private async Task CompleteRouteIfDoneAsync(ReturnRouteId returnRouteId, CancellationToken cancellationToken)
    {
        var route = await returnRouteRepository.FindByIdAsync(returnRouteId, cancellationToken);
        if (route is null || route.Status != EReturnRouteStatus.InProgress) return;

        var accepted = (await matchProposalRepository.FindByReturnRouteIdAsNoTrackingAsync(route.Id, cancellationToken))
            .Where(proposal => proposal.IsAccepted);
        foreach (var proposal in accepted)
        {
            // Tracked lookups, so a shipment changed in this same unit of work is seen with its new status.
            var shipment = await shipmentRequestRepository.FindByIdAsync(proposal.ShipmentRequestId, cancellationToken);
            if (shipment?.Status is EShipmentRequestStatus.Matched or EShipmentRequestStatus.InTransit) return;
        }

        route.Complete();
    }

    private static Shared.Domain.Model.ValueObjects.GeoLocation ToLocation(AddressPoint point) =>
        new(point.Location.Latitude, point.Location.Longitude);
}
