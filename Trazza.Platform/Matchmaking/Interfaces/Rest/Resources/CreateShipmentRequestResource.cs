namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record CreateShipmentRequestResource(
    string Description,
    decimal WeightKg,
    decimal VolumeM3,
    string PickupAddress,
    GeoLocationResource PickupLocation,
    string DeliveryAddress,
    GeoLocationResource DeliveryLocation,
    DateTimeOffset PickupFrom,
    DateTimeOffset PickupUntil);
