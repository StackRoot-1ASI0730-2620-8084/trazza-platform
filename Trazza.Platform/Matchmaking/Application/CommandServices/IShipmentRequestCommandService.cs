using Trazza.Platform.Matchmaking.Domain.Model.Aggregates;
using Trazza.Platform.Matchmaking.Domain.Model.Commands;
using Trazza.Platform.Shared.Application.Model;

namespace Trazza.Platform.Matchmaking.Application.CommandServices;

public interface IShipmentRequestCommandService
{
    Task<Result<ShipmentRequest>> Handle(CreateShipmentRequestCommand command, CancellationToken cancellationToken);

    Task<Result<ShipmentRequest>> Handle(CancelShipmentRequestCommand command, CancellationToken cancellationToken);
}
