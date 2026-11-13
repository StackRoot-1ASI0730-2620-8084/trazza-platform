using Trazza.Platform.Matchmaking.Domain.Model.Errors;
using Trazza.Platform.Matchmaking.Domain.Model.Events;
using Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Errors;

namespace Trazza.Platform.Matchmaking.Domain.Model.Aggregates;

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
        PublicId = Guid.CreateVersion7();
    }

    public ShipmentRequestId Id { get; private set; }
    public Guid PublicId { get; private set; }
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

    public void RaiseCreated() => AddDomainEvent(new ShipmentRequestCreatedEvent(Id, MerchantProfileId));

    public Error MarkMatched()
    {
        if (!IsOpen) return MatchmakingErrors.ShipmentRequestNotOpen;

        Status = EShipmentRequestStatus.Matched;
        return Error.None;
    }

    public Error Reopen()
    {
        if (IsOpen) return Error.None;
        if (Status != EShipmentRequestStatus.Matched) return MatchmakingErrors.ShipmentRequestNotOpen;

        Status = EShipmentRequestStatus.Open;
        return Error.None;
    }

    public Error MarkInTransit()
    {
        if (Status != EShipmentRequestStatus.Matched) return MatchmakingErrors.ShipmentRequestNotOpen;

        Status = EShipmentRequestStatus.InTransit;
        return Error.None;
    }

    public Error MarkDelivered()
    {
        if (Status != EShipmentRequestStatus.InTransit) return MatchmakingErrors.ShipmentRequestNotOpen;

        Status = EShipmentRequestStatus.Delivered;
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

    internal void AssignGeneratedId(ShipmentRequestId id) => Id = id;

    internal void ApplyLocations(GeoLocation pickup, GeoLocation delivery)
    {
        Pickup = Pickup with { Location = pickup };
        Delivery = Delivery with { Location = delivery };
    }
}
