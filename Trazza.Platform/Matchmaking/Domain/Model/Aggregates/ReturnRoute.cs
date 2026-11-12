using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Events;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Errors;

namespace Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

/// <summary>
///     Return route published by a carrier (table return_routes). The remaining capacity is not stored:
///     it is calculated from the vehicle and the accepted shipments.
/// </summary>
public partial class ReturnRoute : AggregateRoot
{
    public const byte DefaultMaxShipments = 2;

    public ReturnRoute()
    {
        Origin = new AddressPoint();
        Destination = new AddressPoint();
        MaxShipments = DefaultMaxShipments;
    }

    public ReturnRoute(CarrierProfileId carrierProfileId, VehicleId vehicleId, DriverId? driverId,
        AddressPoint origin, AddressPoint destination, DateTimeOffset departureAt, byte maxShipments) : this()
    {
        CarrierProfileId = carrierProfileId;
        VehicleId = vehicleId;
        DriverId = driverId;
        Origin = origin;
        Destination = destination;
        DepartureAt = departureAt;
        MaxShipments = maxShipments;
        Status = EReturnRouteStatus.Published;
    }

    public ReturnRouteId Id { get; private set; }
    public CarrierProfileId CarrierProfileId { get; private set; }
    public VehicleId VehicleId { get; private set; }
    public DriverId? DriverId { get; private set; }
    public AddressPoint Origin { get; private set; }
    public AddressPoint Destination { get; private set; }
    public DateTimeOffset DepartureAt { get; private set; }
    public byte MaxShipments { get; private set; }
    public EReturnRouteStatus Status { get; private set; }

    public bool IsPublished => Status == EReturnRouteStatus.Published;

    /// <summary>
    ///     Raises the published event. Call it after the first save so the generated id is already assigned.
    /// </summary>
    public void RaisePublished() => AddDomainEvent(new ReturnRoutePublishedEvent(Id, CarrierProfileId, DepartureAt));

    /// <summary>
    ///     Checks that the route can take one more shipment without passing the maximum number of shipments
    ///     or the load capacity of the vehicle.
    /// </summary>
    public Error EnsureCanTakeShipment(int acceptedShipments, decimal acceptedWeightKg, Cargo cargo,
        decimal vehicleMaxLoadKg)
    {
        if (Status == EReturnRouteStatus.Full) return MatchmakingErrors.ReturnRouteFull;
        if (!IsPublished) return MatchmakingErrors.ReturnRouteNotPublished;
        if (acceptedShipments >= MaxShipments) return MatchmakingErrors.ReturnRouteFull;
        if (acceptedWeightKg + cargo.WeightKg > vehicleMaxLoadKg) return MatchmakingErrors.CapacityExceeded;
        return Error.None;
    }

    /// <summary>
    ///     Call it after a shipment is accepted. The route becomes full when it reaches its maximum.
    /// </summary>
    public Error RegisterAcceptedShipment(int acceptedShipmentsAfter) =>
        acceptedShipmentsAfter >= MaxShipments ? MarkFull() : Error.None;

    public Error MarkFull()
    {
        if (!IsPublished) return MatchmakingErrors.ReturnRouteNotPublished;

        Status = EReturnRouteStatus.Full;
        AddDomainEvent(new ReturnRouteFullEvent(Id));
        return Error.None;
    }

    /// <summary>
    ///     Makes a full route available again, for example when an accepted proposal is cancelled.
    /// </summary>
    public Error Reopen()
    {
        if (IsPublished) return Error.None;
        if (Status != EReturnRouteStatus.Full) return MatchmakingErrors.ReturnRouteNotPublished;

        Status = EReturnRouteStatus.Published;
        return Error.None;
    }

    public Error Cancel()
    {
        if (Status is not (EReturnRouteStatus.Published or EReturnRouteStatus.Full))
            return MatchmakingErrors.ReturnRouteNotCancellable;

        Status = EReturnRouteStatus.Cancelled;
        return Error.None;
    }

    /// <summary>
    ///     The database generates the id. The repository assigns it after inserting the row.
    /// </summary>
    internal void AssignGeneratedId(ReturnRouteId id) => Id = id;

    /// <summary>
    ///     The coordinates live in POINT columns that EF Core does not map. The repository
    ///     reads them with SQL and sets them here.
    /// </summary>
    internal void ApplyLocations(GeoLocation origin, GeoLocation destination)
    {
        Origin = Origin with { Location = origin };
        Destination = Destination with { Location = destination };
    }
}
