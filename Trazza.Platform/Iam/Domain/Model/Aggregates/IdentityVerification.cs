using Trazza.Platform.Iam.Domain.Model.ValueObjects;

namespace Trazza.Platform.Iam.Domain.Model.Aggregates;

public class IdentityVerification
{
    public const string SimulatedProvider = "RENIEC_SIMULATED";

    protected IdentityVerification()
    {
        Provider = SimulatedProvider;
    }

    public IdentityVerification(ulong userId, string? documentFrontS3Key, string? documentBackS3Key, DateTime now)
    {
        UserId = userId;
        Provider = SimulatedProvider;
        Status = VerificationStatus.Submitted;
        DocumentFrontS3Key = documentFrontS3Key;
        DocumentBackS3Key = documentBackS3Key;
        SubmittedAt = now;
    }

    public ulong Id { get; private set; }
    public ulong UserId { get; private set; }
    public string Provider { get; private set; }
    public VerificationStatus Status { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? DocumentFrontS3Key { get; private set; }
    public string? DocumentBackS3Key { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    public void Validate(string externalReference, DateTime now)
    {
        Status = VerificationStatus.Validated;
        ExternalReference = externalReference;
        ReviewedAt = now;
    }

    public void Reject(string reason, DateTime now)
    {
        Status = VerificationStatus.Rejected;
        RejectionReason = reason;
        ReviewedAt = now;
    }
}
