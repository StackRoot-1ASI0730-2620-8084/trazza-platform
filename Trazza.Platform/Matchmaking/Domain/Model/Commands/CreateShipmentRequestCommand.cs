using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

/// <summary>
///     A merchant asks for a shipment to be carried.
/// </summary>
public record CreateShipmentRequestCommand(
    UserId UserId,
    string Description,
    Cargo Cargo,
    AddressPoint Pickup,
    AddressPoint Delivery,
    TimeWindow PickupWindow);
