using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Entities;

namespace Trazza.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     Driver of a fleet (<c>drivers</c>). Drivers have no account: only name, optional sex and experience (1 to 5 stars).
/// </summary>
public class Driver : IAuditableEntity
{
    protected Driver()
    {
        FullName = string.Empty;
    }

    public Driver(ulong carrierProfileId, string fullName, DriverSex? sex, byte experienceStars)
    {
        CarrierProfileId = carrierProfileId;
        FullName = fullName;
        Sex = sex;
        ExperienceStars = experienceStars;
        IsActive = true;
    }

    public ulong Id { get; private set; }
    public ulong CarrierProfileId { get; private set; }
    public string FullName { get; private set; }
    public DriverSex? Sex { get; private set; }
    public byte ExperienceStars { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    DateTimeOffset? IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value?.UtcDateTime ?? default; }
    DateTimeOffset? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value?.UtcDateTime ?? default; }

    public void Update(string fullName, DriverSex? sex, byte experienceStars, bool isActive)
    {
        FullName = fullName;
        Sex = sex;
        ExperienceStars = experienceStars;
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;
}

/// <summary>
///     Truck of a fleet (<c>vehicles</c>): plate, model, maximum load and cargo box dimensions.
/// </summary>
public class Vehicle : IAuditableEntity
{
    protected Vehicle()
    {
        Plate = string.Empty;
        Model = string.Empty;
    }

    public Vehicle(ulong carrierProfileId, string plate, string model, decimal maxLoadKg, decimal cargoLengthM,
        decimal cargoWidthM, decimal cargoHeightM, decimal cargoVolumeM3, string? photoS3Key)
    {
        PublicId = Guid.CreateVersion7();
        CarrierProfileId = carrierProfileId;
        IsActive = true;
        Plate = plate;
        Model = model;
        Update(plate, model, maxLoadKg, cargoLengthM, cargoWidthM, cargoHeightM, cargoVolumeM3, photoS3Key, true);
    }

    public ulong Id { get; private set; }
    public Guid PublicId { get; private set; }
    public ulong CarrierProfileId { get; private set; }
    public string Plate { get; private set; }
    public string Model { get; private set; }
    public decimal MaxLoadKg { get; private set; }
    public decimal CargoLengthM { get; private set; }
    public decimal CargoWidthM { get; private set; }
    public decimal CargoHeightM { get; private set; }
    public decimal CargoVolumeM3 { get; private set; }
    public string? PhotoS3Key { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    DateTimeOffset? IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value?.UtcDateTime ?? default; }
    DateTimeOffset? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value?.UtcDateTime ?? default; }

    public void Update(string plate, string model, decimal maxLoadKg, decimal cargoLengthM, decimal cargoWidthM,
        decimal cargoHeightM, decimal cargoVolumeM3, string? photoS3Key, bool isActive)
    {
        Plate = plate;
        Model = model;
        MaxLoadKg = maxLoadKg;
        CargoLengthM = cargoLengthM;
        CargoWidthM = cargoWidthM;
        CargoHeightM = cargoHeightM;
        CargoVolumeM3 = cargoVolumeM3;
        PhotoS3Key = photoS3Key;
        IsActive = isActive;
    }

    public void Deactivate() => IsActive = false;
}

/// <summary>
///     Frequent address of a user (<c>saved_addresses</c>), e.g. "Almacén" or "Tienda Gamarra".
///     It has a spatial column, so its repository uses SQL instead of EF Core change tracking.
/// </summary>
public record SavedAddress(
    ulong Id,
    ulong UserId,
    string Label,
    string Address,
    string? District,
    double Latitude,
    double Longitude,
    DateTime CreatedAt);
