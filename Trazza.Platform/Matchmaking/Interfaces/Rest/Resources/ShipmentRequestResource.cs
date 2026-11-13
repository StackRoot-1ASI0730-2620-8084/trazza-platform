namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record ShipmentRequestResource(
    Guid Id,
    ulong MerchantProfileId,
    string Description,
    decimal WeightKg,
    decimal VolumeM3,
    string PickupAddress,
    GeoLocationResource PickupLocation,
    string DeliveryAddress,
    GeoLocationResource DeliveryLocation,
    DateTimeOffset PickupFrom,
    DateTimeOffset PickupUntil,
    string Status,
    DateTimeOffset? CancelledAt,
    string? CancellationReason,
    DateTimeOffset? CreatedAt);
