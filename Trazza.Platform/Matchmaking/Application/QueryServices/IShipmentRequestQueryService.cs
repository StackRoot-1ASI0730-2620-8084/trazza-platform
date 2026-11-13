using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IShipmentRequestQueryService
{
    Task<ShipmentRequest?> Handle(GetShipmentRequestByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<ShipmentRequest>> Handle(GetShipmentRequestsByMerchantQuery query,
        CancellationToken cancellationToken);

    Task<IEnumerable<ShipmentRequest>> Handle(SearchOpenShipmentRequestsQuery query,
        CancellationToken cancellationToken);

    Task<IEnumerable<ShipmentRequest>> Handle(GetOpenShipmentRequestsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Fails with route not found when the route does not exist or belongs to another carrier.
    /// </summary>
    Task<Result<IEnumerable<ShipmentRequest>>> Handle(GetSuggestedShipmentRequestsQuery query,
        CancellationToken cancellationToken);
}
