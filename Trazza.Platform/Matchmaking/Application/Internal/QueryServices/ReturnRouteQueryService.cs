using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Application.Model;
using Trazza.Platform.Matchmaking.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Application.Internal.QueryServices;

public class ReturnRouteQueryService(
    IReturnRouteRepository returnRouteRepository,
    IShipmentRequestRepository shipmentRequestRepository,
    IProfilesService profilesService)
    : IReturnRouteQueryService
{
    public async Task<ReturnRoute?> Handle(GetReturnRouteByIdQuery query, CancellationToken cancellationToken)
    {
        return await returnRouteRepository.FindByIdAsNoTrackingAsync(query.ReturnRouteId, cancellationToken);
    }

    public async Task<IEnumerable<ReturnRoute>> Handle(SearchReturnRoutesQuery query,
        CancellationToken cancellationToken)
    {
        return await returnRouteRepository.SearchPublishedAsNoTrackingAsync(query.Pickup, query.Delivery,
            query.PickupWindow, query.MaxDistanceKm, cancellationToken);
    }

    public async Task<IEnumerable<ReturnRoute>> Handle(GetReturnRoutesByCarrierQuery query,
        CancellationToken cancellationToken)
    {
        var carrierProfileId = await profilesService.FetchCarrierProfileIdByUserId(query.UserId, cancellationToken);
        if (carrierProfileId is null) return [];

        return await returnRouteRepository.FindByCarrierProfileIdAsNoTrackingAsync(carrierProfileId.Value,
            cancellationToken);
    }

    public async Task<Result<IEnumerable<ReturnRoute>>> Handle(GetSuggestedReturnRoutesQuery query,
        CancellationToken cancellationToken)
    {
        var merchantProfileId = await profilesService.FetchMerchantProfileIdByUserId(query.UserId, cancellationToken);
        var request = await shipmentRequestRepository.FindByIdAsNoTrackingAsync(query.ShipmentRequestId,
            cancellationToken);
        if (request is null || request.MerchantProfileId != merchantProfileId)
            return MatchmakingErrors.ShipmentRequestNotFound;
        if (request.Status != EShipmentRequestStatus.Open) return Result<IEnumerable<ReturnRoute>>.Success([]);

        var routes = await returnRouteRepository.SearchPublishedAsNoTrackingAsync(request.Pickup.Location,
            request.Delivery.Location, request.PickupWindow, SearchReturnRoutesQuery.DefaultMaxDistanceKm,
            cancellationToken);
        return Result<IEnumerable<ReturnRoute>>.Success(routes);
    }
}
