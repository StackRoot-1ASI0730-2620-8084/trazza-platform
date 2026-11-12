namespace Trazza.Platform.Matchmaking.Interfaces.Rest.Resources;

public record PublishReturnRouteResource(
    ulong VehicleId,
    ulong? DriverId,
    string OriginAddress,
    GeoLocationResource OriginLocation,
    string DestinationAddress,
    GeoLocationResource DestinationLocation,
    DateTimeOffset DepartureAt,
    byte MaxShipments);
