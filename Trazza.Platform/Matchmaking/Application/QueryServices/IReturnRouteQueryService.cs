using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Queries;

namespace Trazza.Platform.Matchmaking.Application.QueryServices;

public interface IReturnRouteQueryService
{
    Task<ReturnRoute?> Handle(GetReturnRouteByIdQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<ReturnRoute>> Handle(SearchReturnRoutesQuery query, CancellationToken cancellationToken);

    Task<IEnumerable<ReturnRoute>> Handle(GetReturnRoutesByCarrierQuery query, CancellationToken cancellationToken);
}
