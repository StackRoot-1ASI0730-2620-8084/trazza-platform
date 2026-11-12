using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Events;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Errors;

namespace Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

/// <summary>
///     Shipment request created by a merchant (table shipment_requests).
/// </summary>
public partial class ShipmentRequest : AggregateRoot
{
    public const int MaxDescriptionLength = 500;
    public const int MaxCancellationReasonLength = 255;

    public ShipmentRequest()
    {
        Description = string.Empty;
        Cargo = new Cargo();
        Pickup = new AddressPoint();
        Delivery = new AddressPoint();
        PickupWindow = new TimeWindow();
    }

    public ShipmentRequest(MerchantProfileId merchantProfileId, string description, Cargo cargo,
        AddressPoint pickup, AddressPoint delivery, TimeWindow pickupWindow) : this()
    {
        MerchantProfileId = merchantProfileId;
        Description = description;
        Cargo = cargo;
        Pickup = pickup;
        Delivery = delivery;
        PickupWindow = pickupWindow;
        Status = EShipmentRequestStatus.Open;
    }

    public ShipmentRequestId Id { get; private set; }
    public MerchantProfileId MerchantProfileId { get; private set; }
    public string Description { get; private set; }
    public Cargo Cargo { get; private set; }
    public AddressPoint Pickup { get; private set; }
    public AddressPoint Delivery { get; private set; }
    public TimeWindow PickupWindow { get; private set; }
    public EShipmentRequestStatus Status { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public bool IsOpen => Status == EShipmentRequestStatus.Open;

    /// <summary>
    ///     Raises the created event. Call it after the first save so the generated id is already assigned.
    /// </summary>
    public void RaiseCreated() => AddDomainEvent(new ShipmentRequestCreatedEvent(Id, MerchantProfileId));

    public Error MarkMatched()
    {
        if (!IsOpen) return MatchmakingErrors.ShipmentRequestNotOpen;

        Status = EShipmentRequestStatus.Matched;
        return Error.None;
    }

    /// <summary>
    ///     Makes a matched request open again, for example when the accepted proposal is cancelled.
    /// </summary>
    public Error Reopen()
    {
        if (IsOpen) return Error.None;
        if (Status != EShipmentRequestStatus.Matched) return MatchmakingErrors.ShipmentRequestNotOpen;

        Status = EShipmentRequestStatus.Open;
        return Error.None;
    }

    public Error Cancel(string? reason, DateTimeOffset now)
    {
        if (Status is not (EShipmentRequestStatus.Open or EShipmentRequestStatus.Matched))
            return MatchmakingErrors.ShipmentRequestNotCancellable;

        Status = EShipmentRequestStatus.Cancelled;
        CancelledAt = now;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        return Error.None;
    }

    /// <summary>
    ///     The database generates the id. The repository assigns it after inserting the row.
    /// </summary>
    internal void AssignGeneratedId(ShipmentRequestId id) => Id = id;

    /// <summary>
    ///     The coordinates live in POINT columns that EF Core does not map. The repository
    ///     reads them with SQL and sets them here.
    /// </summary>
    internal void ApplyLocations(GeoLocation pickup, GeoLocation delivery)
    {
        Pickup = Pickup with { Location = pickup };
        Delivery = Delivery with { Location = delivery };
    }
}
