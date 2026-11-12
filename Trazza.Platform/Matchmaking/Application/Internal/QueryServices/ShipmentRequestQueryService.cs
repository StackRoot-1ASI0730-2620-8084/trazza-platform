using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Matchmaking.Domain.Repositories;

namespace Trazza.Platform.Matchmaking.Application.Internal.QueryServices;

public class ShipmentRequestQueryService(
    IShipmentRequestRepository shipmentRequestRepository,
    IProfilesService profilesService)
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
}
