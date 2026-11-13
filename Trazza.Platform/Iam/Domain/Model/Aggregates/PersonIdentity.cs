using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Entities;

namespace Trazza.Platform.Iam.Domain.Model.Aggregates;

public class PersonIdentity : IAuditableEntity
{
    protected PersonIdentity()
    {
        FullName = string.Empty;
        DniEncrypted = [];
        DniHash = [];
        DniLast3 = string.Empty;
        PhoneEncrypted = [];
    }

    public PersonIdentity(ulong userId, string fullName, byte[] dniEncrypted, byte[] dniHash, string dniLast3,
        byte[] phoneEncrypted)
    {
        UserId = userId;
        FullName = fullName;
        DniEncrypted = dniEncrypted;
        DniHash = dniHash;
        DniLast3 = dniLast3;
        PhoneEncrypted = phoneEncrypted;
        KycStatus = KycStatus.Pending;
    }

    public ulong UserId { get; private set; }
    public string FullName { get; private set; }
    public byte[] DniEncrypted { get; private set; }
    public byte[] DniHash { get; private set; }
    public string DniLast3 { get; private set; }
    public byte[] PhoneEncrypted { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    DateTimeOffset? IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value?.UtcDateTime ?? default; }
    DateTimeOffset? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value?.UtcDateTime ?? default; }

    public void UpdateContact(string fullName, byte[] phoneEncrypted)
    {
        FullName = fullName;
        PhoneEncrypted = phoneEncrypted;
    }

    public void SetKycStatus(KycStatus status) => KycStatus = status;
}
