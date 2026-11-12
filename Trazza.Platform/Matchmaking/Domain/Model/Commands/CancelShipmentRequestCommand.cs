using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

public record CancelShipmentRequestCommand(UserId UserId, ShipmentRequestId ShipmentRequestId, string? Reason);
