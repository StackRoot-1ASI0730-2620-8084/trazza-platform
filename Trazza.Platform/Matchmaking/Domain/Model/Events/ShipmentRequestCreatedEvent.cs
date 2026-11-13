using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Matchmaking.Domain.Model.Events;

public record ShipmentRequestCreatedEvent(ShipmentRequestId ShipmentRequestId, MerchantProfileId MerchantProfileId)
    : IEvent;
