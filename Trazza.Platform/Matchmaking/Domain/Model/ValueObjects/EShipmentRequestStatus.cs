namespace Trazza.Platform.Matchmaking.Domain.Model.ValueObjects;

/// <summary>
///     Status of a shipment request. Stored as OPEN, MATCHED, IN_TRANSIT, DELIVERED or CANCELLED.
/// </summary>
public enum EShipmentRequestStatus
{
    Open,
    Matched,
    InTransit,
    Delivered,
    Cancelled
}
