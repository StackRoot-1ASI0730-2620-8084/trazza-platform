using Trazza.Platform.Execution.Domain.Model.Aggregates;
using Trazza.Platform.Execution.Domain.Model.Commands;
using Trazza.Platform.Execution.Domain.Model.ValueObjects;
using Trazza.Platform.Execution.Domain.Repositories;
using Trazza.Platform.Execution.Domain.Services;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Execution.Application.Internal.CommandServices;

public class IncidentCommandService(
    TripAccessService tripAccessService,
    IIncidentRepository incidentRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IIncidentCommandService
{
    private const int MaxTextLength = 1000;
    private const int MaxS3KeyLength = 512;

    public async Task<Result<Incident, CommandError>> Handle(ReportIncidentCommand command, CancellationToken ct = default)
    {
        var access = await tripAccessService.FindAsync(command.UserId, command.TripId, ct);
        if (access is null) return CommandError.NotFound("trip-not-found");
        if (access.Trip.Status is TripStatus.Scheduled or TripStatus.Cancelled)
            return CommandError.Conflict("trip-status-invalid");
        if (string.IsNullOrWhiteSpace(command.Description) || command.Description.Trim().Length > MaxTextLength)
            return CommandError.Validation("description-invalid");

        var incident = new Incident(access.Trip.Id, command.UserId, command.Type, command.Description,
            timeProvider.GetUtcNow().UtcDateTime);
        await incidentRepository.AddAsync(incident, ct);
        await unitOfWork.CompleteAsync(ct);
        return incident;
    }

    public async Task<Result<Incident, CommandError>> Handle(AddIncidentEvidenceCommand command,
        CancellationToken ct = default)
    {
        var incident = await FindAsync(command.UserId, command.TripId, command.IncidentId, ct);
        if (incident is null) return CommandError.NotFound("incident-not-found");
        if (incident.Status == IncidentStatus.Resolved) return CommandError.Conflict("incident-resolved");
        if (string.IsNullOrWhiteSpace(command.S3Key) || command.S3Key.Trim().Length > MaxS3KeyLength
                                                     || command.S3Key.Trim().Contains(' '))
            return CommandError.Validation("s3-key-invalid");

        incident.AddEvidence(command.S3Key, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.CompleteAsync(ct);
        return incident;
    }

    public async Task<Result<Incident, CommandError>> Handle(ResolveIncidentCommand command, CancellationToken ct = default)
    {
        var incident = await FindAsync(command.UserId, command.TripId, command.IncidentId, ct);
        if (incident is null) return CommandError.NotFound("incident-not-found");
        if (incident.Status == IncidentStatus.Resolved) return CommandError.Conflict("incident-resolved");
        if (string.IsNullOrWhiteSpace(command.Resolution) || command.Resolution.Trim().Length > MaxTextLength)
            return CommandError.Validation("resolution-invalid");

        incident.Resolve(command.Resolution, timeProvider.GetUtcNow().UtcDateTime);
        await unitOfWork.CompleteAsync(ct);
        return incident;
    }

    private async Task<Incident?> FindAsync(ulong userId, Guid tripId, ulong incidentId, CancellationToken ct)
    {
        var access = await tripAccessService.FindAsync(userId, tripId, ct);
        return access is null ? null : await incidentRepository.FindWithEvidencesAsync(incidentId, access.Trip.Id, ct);
    }
}
