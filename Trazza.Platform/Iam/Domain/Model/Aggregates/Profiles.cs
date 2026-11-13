using Trazza.Platform.Shared.Domain.Model.Entities;

namespace Trazza.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     Carrier profile (<c>carrier_profiles</c>): owner of the fleet (vehicles and drivers).
/// </summary>
public class CarrierProfile : IAuditableEntity
{
    protected CarrierProfile()
    {
    }

    public CarrierProfile(ulong userId)
    {
        UserId = userId;
    }

    public ulong Id { get; private set; }
    public ulong UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    DateTimeOffset? IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value?.UtcDateTime ?? default; }
    DateTimeOffset? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value?.UtcDateTime ?? default; }
}

/// <summary>
///     Merchant profile (<c>merchant_profiles</c>). The merchant needs a RUC (here) and a DNI (person identity).
/// </summary>
public class MerchantProfile : IAuditableEntity
{
    protected MerchantProfile()
    {
        BusinessName = string.Empty;
        Ruc = string.Empty;
        BusinessType = string.Empty;
    }

    public MerchantProfile(ulong userId, string businessName, string ruc, string businessType)
    {
        UserId = userId;
        BusinessName = businessName;
        Ruc = ruc;
        BusinessType = businessType;
    }

    public ulong Id { get; private set; }
    public ulong UserId { get; private set; }
    public string BusinessName { get; private set; }
    public string Ruc { get; private set; }
    public string BusinessType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    DateTimeOffset? IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value?.UtcDateTime ?? default; }
    DateTimeOffset? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value?.UtcDateTime ?? default; }

    public void Update(string businessName, string businessType)
    {
        BusinessName = businessName;
        BusinessType = businessType;
    }
}
