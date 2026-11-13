using Trazza.Platform.Execution.Domain.Model.ValueObjects;

namespace Trazza.Platform.Execution.Domain.Model.Aggregates;

public class Incident
{
    private readonly List<IncidentEvidence> _evidences = [];

    protected Incident()
    {
        Description = string.Empty;
    }

    public Incident(ulong tripId, ulong reportedByUserId, IncidentType type, string description, DateTime now)
    {
        TripId = tripId;
        ReportedByUserId = reportedByUserId;
        Type = type;
        Description = description.Trim();
        Status = IncidentStatus.Open;
        ReportedAt = now;
    }

    public ulong Id { get; private set; }
    public ulong TripId { get; private set; }
    public ulong ReportedByUserId { get; private set; }
    public IncidentType Type { get; private set; }
    public string Description { get; private set; }
    public IncidentStatus Status { get; private set; }
    public string? Resolution { get; private set; }
    public DateTime ReportedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public IReadOnlyCollection<IncidentEvidence> Evidences => _evidences;

    public IncidentEvidence AddEvidence(string s3Key, DateTime now)
    {
        var evidence = new IncidentEvidence(Id, s3Key.Trim(), now);
        _evidences.Add(evidence);
        if (Status == IncidentStatus.Open) Status = IncidentStatus.InReview;
        return evidence;
    }

    public void Resolve(string resolution, DateTime now)
    {
        Status = IncidentStatus.Resolved;
        Resolution = resolution.Trim();
        ResolvedAt = now;
    }
}

public class IncidentEvidence
{
    protected IncidentEvidence()
    {
        S3Key = string.Empty;
    }

    public IncidentEvidence(ulong incidentId, string s3Key, DateTime uploadedAt)
    {
        IncidentId = incidentId;
        S3Key = s3Key;
        UploadedAt = uploadedAt;
    }

    public ulong Id { get; private set; }
    public ulong IncidentId { get; private set; }
    public string S3Key { get; private set; }
    public DateTime UploadedAt { get; private set; }
}

public class IotDevice
{
    protected IotDevice()
    {
        SerialNumber = string.Empty;
    }

    public ulong Id { get; private set; }
    public string SerialNumber { get; private set; }
    public IotDeviceStatus Status { get; private set; }
    public ulong? AssignedVehicleId { get; private set; }
    public DateTime? LoanedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void LoanTo(ulong vehicleId, DateTime now)
    {
        Status = IotDeviceStatus.Loaned;
        AssignedVehicleId = vehicleId;
        LoanedAt = now;
    }

    public void Return()
    {
        Status = IotDeviceStatus.Available;
        AssignedVehicleId = null;
        LoanedAt = null;
    }
}