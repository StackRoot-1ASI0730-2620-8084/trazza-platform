using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.CommandServices;

public interface IReturnRouteCommandService
{
    Task<Result<ReturnRoute>> Handle(PublishReturnRouteCommand command, CancellationToken cancellationToken);

    Task<Result<ReturnRoute>> Handle(CancelReturnRouteCommand command, CancellationToken cancellationToken);
}
