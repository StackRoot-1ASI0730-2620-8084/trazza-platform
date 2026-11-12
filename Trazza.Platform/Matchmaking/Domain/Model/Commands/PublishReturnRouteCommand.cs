using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

namespace Trazza.Platform.Matchmaking.Domain.Model.Commands;

/// <summary>
///     A carrier publishes a return route with one of its vehicles.
/// </summary>
public record PublishReturnRouteCommand(
    UserId UserId,
    VehicleId VehicleId,
    DriverId? DriverId,
    AddressPoint Origin,
    AddressPoint Destination,
    DateTimeOffset DepartureAt,
    byte MaxShipments);
