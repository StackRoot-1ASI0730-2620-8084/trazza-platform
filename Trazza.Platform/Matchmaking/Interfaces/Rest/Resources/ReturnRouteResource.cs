namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record ReturnRouteResource(
    ulong Id,
    ulong CarrierProfileId,
    Guid? VehicleId,
    ulong? DriverId,
    string OriginAddress,
    GeoLocationResource OriginLocation,
    string DestinationAddress,
    GeoLocationResource DestinationLocation,
    DateTimeOffset DepartureAt,
    byte MaxShipments,
    string Status,
    DateTimeOffset? CreatedAt);
