namespace Trazza.Platform.Execution.Domain.Model.ValueObjects;

public enum TripStatus
{
    Scheduled,
    PickedUp,
    InTransit,
    Delivered,
    Confirmed,
    Cancelled
}

public enum TrackingSource
{
    Phone,
    Iot
}

public enum IncidentType
{
    Damaged,
    Incomplete,
    Delay,
    Other
}

public enum IncidentStatus
{
    Open,
    InReview,
    Resolved
}

public enum IotDeviceStatus
{
    Available,
    Loaned,
    Maintenance,
    Retired
}