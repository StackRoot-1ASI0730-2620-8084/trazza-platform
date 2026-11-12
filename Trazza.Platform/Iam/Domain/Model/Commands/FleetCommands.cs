using TrazzaPlatform.API.Iam.Domain.Model.ValueObjects;

namespace TrazzaPlatform.API.Iam.Domain.Model.Commands;

public record VehicleData(
    string Plate,
    string Model,
    decimal MaxLoadKg,
    decimal CargoLengthM,
    decimal CargoWidthM,
    decimal CargoHeightM,
    decimal? CargoVolumeM3,
    string? PhotoS3Key,
    bool IsActive);

public record CreateVehicleCommand(ulong UserId, VehicleData Data);

public record UpdateVehicleCommand(ulong UserId, Guid VehicleId, VehicleData Data);

public record DeactivateVehicleCommand(ulong UserId, Guid VehicleId);

public record DriverData(string FullName, DriverSex? Sex, int ExperienceStars, bool IsActive);

public record CreateDriverCommand(ulong UserId, DriverData Data);

public record UpdateDriverCommand(ulong UserId, ulong DriverId, DriverData Data);

public record DeactivateDriverCommand(ulong UserId, ulong DriverId);

public record CreateSavedAddressCommand(
    ulong UserId,
    string Label,
    string Address,
    string? District,
    double Latitude,
    double Longitude);

public record DeleteSavedAddressCommand(ulong UserId, ulong SavedAddressId);
