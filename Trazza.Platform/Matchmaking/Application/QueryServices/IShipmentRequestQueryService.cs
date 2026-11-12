using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IShipmentRequestQueryService
{
    Task<ShipmentRequest?> Handle(GetShipmentRequestByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<ShipmentRequest>> Handle(GetShipmentRequestsByMerchantQuery query,
        CancellationToken cancellationToken);

    Task<IEnumerable<ShipmentRequest>> Handle(SearchOpenShipmentRequestsQuery query,
        CancellationToken cancellationToken);
}
