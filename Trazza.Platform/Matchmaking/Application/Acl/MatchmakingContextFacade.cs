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
    IShipmentRequestRepository shipmentRequestRepository)
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

        return proposals.Where(proposal => proposal.Status == EMatchProposalStatus.Accepted)
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

            contexts.Add(new TripContextSnapshot(proposal.Id, proposal.Id, route.Id, shipment.Id,
                route.CarrierProfileId, shipment.MerchantProfileId, route.VehicleId, route.DriverId?.Value,
                route.Origin.Address, ToLocation(route.Origin), route.Destination.Address,
                ToLocation(route.Destination), shipment.Pickup.Address, ToLocation(shipment.Pickup),
                shipment.Delivery.Address, ToLocation(shipment.Delivery), route.DepartureAt.UtcDateTime,
                shipment.Description, shipment.Cargo.WeightKg, shipment.Cargo.VolumeM3));
        }

        return contexts;
    }

    private static Shared.Domain.Model.ValueObjects.GeoLocation ToLocation(AddressPoint point) =>
        new(point.Location.Latitude, point.Location.Longitude);
}
