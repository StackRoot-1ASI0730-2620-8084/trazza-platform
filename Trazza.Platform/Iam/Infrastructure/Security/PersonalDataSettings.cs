namespace TrazzaPlatform.API.Iam.Infrastructure.Security;

/// <summary>
///     Keys (Base64, 32 bytes each) used to protect personal data. In production they come from environment
///     variables (ideally loaded from AWS KMS / Secrets Manager); they must never be committed.
/// </summary>
public class PersonalDataSettings
{
    public string EncryptionKey { get; set; } = string.Empty;
    public string HashKey { get; set; } = string.Empty;
}
