using Trazza.Platform.Execution.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.Entities;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Execution.Domain.Model.Aggregates;

/// <summary>
///     Execution of an accepted proposal: SCHEDULED, PICKED_UP, IN_TRANSIT, DELIVERED and CONFIRMED by the merchant,
///     or CANCELLED. Every change is written to <c>trip_status_history</c>.
/// </summary>
public class Trip : IAuditableEntity
{
    private readonly List<TripStatusChange> _history = [];

    protected Trip()
    {
        Currency = string.Empty;
    }

    public Trip(ulong matchProposalId, ulong? driverId, decimal agreedRate, string currency, ulong createdByUserId,
        DateTime now)
    {
        PublicId = Guid.CreateVersion7();
        MatchProposalId = matchProposalId;
        DriverId = driverId;
        AgreedRate = agreedRate;
        Currency = currency;
        Status = TripStatus.Scheduled;
        _history.Add(new TripStatusChange(null, Status, createdByUserId, now));
    }

    public ulong Id { get; private set; }
    public Guid PublicId { get; private set; }
    public ulong MatchProposalId { get; private set; }
    public ulong? DriverId { get; private set; }
    public decimal AgreedRate { get; private set; }
    public string Currency { get; private set; }
    public TripStatus Status { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public ulong? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }
    public decimal? TotalDistanceKm { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public IReadOnlyCollection<TripStatusChange> History => _history;

    public bool IsClosed => Status is TripStatus.Confirmed or TripStatus.Cancelled;

    public bool IsOnTheRoad => Status is TripStatus.PickedUp or TripStatus.InTransit;

    public Result<Trip, CommandError> AssignDriver(ulong driverId)
    {
        if (Status is not (TripStatus.Scheduled or TripStatus.PickedUp)) return CommandError.Conflict("trip-status-invalid");
        DriverId = driverId;
        return this;
    }

    public Result<Trip, CommandError> PickUp(ulong userId, DateTime now)
    {
        if (Status != TripStatus.Scheduled) return CommandError.Conflict("trip-status-invalid");
        PickedUpAt = now;
        return ChangeStatus(TripStatus.PickedUp, userId, now);
    }

    /// <summary>The first GPS point after the pick-up moves the trip to IN_TRANSIT.</summary>
    public void RegisterMovement(double addedDistanceKm, ulong userId, DateTime now)
    {
        if (addedDistanceKm > 0)
            TotalDistanceKm = Math.Round((TotalDistanceKm ?? 0) + (decimal)addedDistanceKm, 2);
        if (Status == TripStatus.PickedUp) ChangeStatus(TripStatus.InTransit, userId, now);
    }

    public Result<Trip, CommandError> Deliver(ulong userId, DateTime now)
    {
        if (!IsOnTheRoad) return CommandError.Conflict("trip-status-invalid");
        DeliveredAt = now;
        return ChangeStatus(TripStatus.Delivered, userId, now);
    }

    public Result<Trip, CommandError> Confirm(ulong userId, DateTime now)
    {
        if (Status != TripStatus.Delivered) return CommandError.Conflict("trip-status-invalid");
        ConfirmedAt = now;
        return ChangeStatus(TripStatus.Confirmed, userId, now);
    }

    /// <summary>Only a trip that has not been picked up can be cancelled.</summary>
    public Result<Trip, CommandError> Cancel(ulong userId, string reason, DateTime now)
    {
        if (Status != TripStatus.Scheduled) return CommandError.Conflict("trip-status-invalid");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 255)
            return CommandError.Validation("cancellation-reason-required");
        CancelledAt = now;
        CancelledByUserId = userId;
        CancellationReason = reason.Trim();
        return ChangeStatus(TripStatus.Cancelled, userId, now);
    }

    private Trip ChangeStatus(TripStatus status, ulong userId, DateTime now)
    {
        _history.Add(new TripStatusChange(Status, status, userId, now) { TripId = Id });
        Status = status;
        return this;
    }
}

/// <summary>Row of <c>trip_status_history</c>.</summary>
public class TripStatusChange
{
    protected TripStatusChange()
    {
        ToStatus = string.Empty;
    }

    public TripStatusChange(TripStatus? fromStatus, TripStatus toStatus, ulong? changedByUserId, DateTime changedAt)
    {
        FromStatus = fromStatus.HasValue ? EnumText.ToText(fromStatus.Value) : null;
        ToStatus = EnumText.ToText(toStatus);
        ChangedByUserId = changedByUserId;
        ChangedAt = changedAt;
    }

    public ulong Id { get; private set; }
    public ulong TripId { get; internal set; }
    public string? FromStatus { get; private set; }
    public string ToStatus { get; private set; }
    public ulong? ChangedByUserId { get; private set; }
    public DateTime ChangedAt { get; private set; }
}

/// <summary>GPS point of an active trip. Stored with SQL (POINT column).</summary>
public record TrackingPoint(
    ulong Id,
    ulong TripId,
    double Latitude,
    double Longitude,
    decimal? SpeedKmh,
    TrackingSource Source,
    DateTime RecordedAt);

/// <summary>The truck left the expected path by more than the allowed distance. Stored with SQL.</summary>
public record DeviationAlert(
    ulong Id,
    ulong TripId,
    double Latitude,
    double Longitude,
    decimal DeviationKm,
    DateTime DetectedAt,
    DateTime? NotifiedAt,
    DateTime? AcknowledgedAt);
