using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IReturnRouteQueryService
{
    Task<ReturnRoute?> Handle(GetReturnRouteByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<ReturnRoute>> Handle(SearchReturnRoutesQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<ReturnRoute>> Handle(GetReturnRoutesByCarrierQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Fails with shipment request not found when the request does not exist or belongs to another merchant.
    /// </summary>
    Task<Result<IEnumerable<ReturnRoute>>> Handle(GetSuggestedReturnRoutesQuery query,
        CancellationToken cancellationToken);
}
