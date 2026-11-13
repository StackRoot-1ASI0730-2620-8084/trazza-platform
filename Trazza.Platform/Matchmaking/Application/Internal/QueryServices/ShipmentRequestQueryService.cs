using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Matchmaking.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Application.Internal.QueryServices;

public class ShipmentRequestQueryService(
    IShipmentRequestRepository shipmentRequestRepository,
    IReturnRouteRepository returnRouteRepository,
    IProfilesService profilesService,
    TimeProvider timeProvider)
    : IShipmentRequestQueryService
{
    public async Task<ShipmentRequest?> Handle(GetShipmentRequestByIdQuery query, CancellationToken cancellationToken)
    {
        return await shipmentRequestRepository.FindByIdAsNoTrackingAsync(query.ShipmentRequestId, cancellationToken);
    }

    public async Task<IEnumerable<ShipmentRequest>> Handle(GetShipmentRequestsByMerchantQuery query,
        CancellationToken cancellationToken)
    {
        var merchantProfileId = await profilesService.FetchMerchantProfileIdByUserId(query.UserId, cancellationToken);
        if (merchantProfileId is null) return [];

        return await shipmentRequestRepository.FindByMerchantProfileIdAsNoTrackingAsync(merchantProfileId.Value,
            cancellationToken);
    }

    public async Task<IEnumerable<ShipmentRequest>> Handle(SearchOpenShipmentRequestsQuery query,
        CancellationToken cancellationToken)
    {
        return await shipmentRequestRepository.SearchOpenAsNoTrackingAsync(query.Origin, query.Destination,
            query.DepartureAt, query.MaxDistanceKm, cancellationToken);
    }

    public async Task<IEnumerable<ShipmentRequest>> Handle(GetOpenShipmentRequestsQuery query,
        CancellationToken cancellationToken)
    {
        return await shipmentRequestRepository.ListOpenAsNoTrackingAsync(timeProvider.GetUtcNow(), cancellationToken);
    }

    public async Task<Result<IEnumerable<ShipmentRequest>>> Handle(GetSuggestedShipmentRequestsQuery query,
        CancellationToken cancellationToken)
    {
        var carrierProfileId = await profilesService.FetchCarrierProfileIdByUserId(query.UserId, cancellationToken);
        var route = await returnRouteRepository.FindByIdAsNoTrackingAsync(query.ReturnRouteId, cancellationToken);
        if (route is null || route.CarrierProfileId != carrierProfileId)
            return MatchmakingErrors.ReturnRouteNotFound;
        if (route.Status != EReturnRouteStatus.Published) return Result<IEnumerable<ShipmentRequest>>.Success([]);

        var requests = await shipmentRequestRepository.SearchOpenAsNoTrackingAsync(route.Origin.Location,
            route.Destination.Location, route.DepartureAt, SearchReturnRoutesQuery.DefaultMaxDistanceKm,
            cancellationToken);
        return Result<IEnumerable<ShipmentRequest>>.Success(requests);
    }
}
