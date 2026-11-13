using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Iam.Domain.Model.Commands;
using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Iam.Domain.Repositories;
using Trazza.Platform.Iam.Domain.Services;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Iam.Application.Internal.CommandServices;

public class FleetCommandService(
    ICarrierProfileRepository carrierProfileRepository,
    IVehicleRepository vehicleRepository,
    IDriverRepository driverRepository,
    ISavedAddressRepository savedAddressRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IFleetCommandService
{
    public const decimal MaxLoadKg = 30000m;
    public const decimal MaxDimensionM = 20m;

    public async Task<Result<Vehicle, CommandError>> Handle(CreateVehicleCommand command, CancellationToken ct = default)
    {
        var profile = await carrierProfileRepository.FindByUserIdAsync(command.UserId, ct);
        if (profile is null) return CommandError.Forbidden("carrier-role-required");
        var data = ValidateVehicle(command.Data);
        if (data.IsFailure) return data.Error!;
        if (await vehicleRepository.ExistsByPlateAsync(data.Value!.Plate, null, ct))
            return CommandError.Conflict("plate-already-registered");

        var value = data.Value;
        var vehicle = new Vehicle(profile.Id, value.Plate, value.Model, value.MaxLoadKg, value.CargoLengthM,
            value.CargoWidthM, value.CargoHeightM, value.CargoVolumeM3!.Value, value.PhotoS3Key);
        await vehicleRepository.AddAsync(vehicle, ct);
        await unitOfWork.CompleteAsync(ct);
        return vehicle;
    }

    public async Task<Result<Vehicle, CommandError>> Handle(UpdateVehicleCommand command, CancellationToken ct = default)
    {
        var owned = await FindOwnedVehicleAsync(command.UserId, command.VehicleId, ct);
        if (owned.IsFailure) return owned.Error!;
        var data = ValidateVehicle(command.Data);
        if (data.IsFailure) return data.Error!;
        var vehicle = owned.Value!;
        if (await vehicleRepository.ExistsByPlateAsync(data.Value!.Plate, vehicle.Id, ct))
            return CommandError.Conflict("plate-already-registered");

        var value = data.Value;
        vehicle.Update(value.Plate, value.Model, value.MaxLoadKg, value.CargoLengthM, value.CargoWidthM,
            value.CargoHeightM, value.CargoVolumeM3!.Value, value.PhotoS3Key, value.IsActive);
        await unitOfWork.CompleteAsync(ct);
        return vehicle;
    }

    public async Task<Result<Vehicle, CommandError>> Handle(DeactivateVehicleCommand command,
        CancellationToken ct = default)
    {
        var owned = await FindOwnedVehicleAsync(command.UserId, command.VehicleId, ct);
        if (owned.IsFailure) return owned.Error!;
        owned.Value!.Deactivate();
        await unitOfWork.CompleteAsync(ct);
        return owned.Value;
    }

    public async Task<Result<Driver, CommandError>> Handle(CreateDriverCommand command, CancellationToken ct = default)
    {
        var profile = await carrierProfileRepository.FindByUserIdAsync(command.UserId, ct);
        if (profile is null) return CommandError.Forbidden("carrier-role-required");
        var data = ValidateDriver(command.Data);
        if (data.IsFailure) return data.Error!;

        var driver = new Driver(profile.Id, data.Value!.FullName, data.Value.Sex, (byte)data.Value.ExperienceStars);
        await driverRepository.AddAsync(driver, ct);
        await unitOfWork.CompleteAsync(ct);
        return driver;
    }

    public async Task<Result<Driver, CommandError>> Handle(UpdateDriverCommand command, CancellationToken ct = default)
    {
        var owned = await FindOwnedDriverAsync(command.UserId, command.DriverId, ct);
        if (owned.IsFailure) return owned.Error!;
        var data = ValidateDriver(command.Data);
        if (data.IsFailure) return data.Error!;

        owned.Value!.Update(data.Value!.FullName, data.Value.Sex, (byte)data.Value.ExperienceStars, data.Value.IsActive);
        await unitOfWork.CompleteAsync(ct);
        return owned.Value;
    }

    public async Task<Result<Driver, CommandError>> Handle(DeactivateDriverCommand command, CancellationToken ct = default)
    {
        var owned = await FindOwnedDriverAsync(command.UserId, command.DriverId, ct);
        if (owned.IsFailure) return owned.Error!;
        owned.Value!.Deactivate();
        await unitOfWork.CompleteAsync(ct);
        return owned.Value;
    }

    public async Task<Result<SavedAddress, CommandError>> Handle(CreateSavedAddressCommand command,
        CancellationToken ct = default)
    {
        var label = (command.Label ?? string.Empty).Trim();
        if (label.Length is < 2 or > 60) return CommandError.Validation("address-label-required");
        var address = (command.Address ?? string.Empty).Trim();
        if (address.Length is < 5 or > 255) return CommandError.Validation("address-street-required");
        var district = string.IsNullOrWhiteSpace(command.District) ? null : command.District.Trim();
        if (district is { Length: > 80 }) return CommandError.Validation("address-district-invalid");
        var location = GeoLocation.Create(command.Latitude, command.Longitude);
        if (location.IsFailure) return location.Error!;

        var saved = await savedAddressRepository.AddAsync(new SavedAddress(0, command.UserId, label, address, district,
            location.Value!.Latitude, location.Value.Longitude, timeProvider.GetUtcNow().UtcDateTime), ct);
        return saved;
    }

    public async Task<Result<bool, CommandError>> Handle(DeleteSavedAddressCommand command, CancellationToken ct = default)
    {
        var deleted = await savedAddressRepository.DeleteAsync(command.SavedAddressId, command.UserId, ct);
        if (!deleted) return CommandError.NotFound("saved-address-not-found");
        return true;
    }

    private async Task<Result<Vehicle, CommandError>> FindOwnedVehicleAsync(ulong userId, Guid vehicleId,
        CancellationToken ct)
    {
        var profile = await carrierProfileRepository.FindByUserIdAsync(userId, ct);
        if (profile is null) return CommandError.Forbidden("carrier-role-required");
        var vehicle = await vehicleRepository.FindByPublicIdAsync(vehicleId, ct);
        if (vehicle is null || vehicle.CarrierProfileId != profile.Id) return CommandError.NotFound("vehicle-not-found");
        return vehicle;
    }

    private async Task<Result<Driver, CommandError>> FindOwnedDriverAsync(ulong userId, ulong driverId,
        CancellationToken ct)
    {
        var profile = await carrierProfileRepository.FindByUserIdAsync(userId, ct);
        if (profile is null) return CommandError.Forbidden("carrier-role-required");
        var driver = await driverRepository.FindByIdAsync(driverId, ct);
        if (driver is null || driver.CarrierProfileId != profile.Id) return CommandError.NotFound("driver-not-found");
        return driver;
    }

    private static Result<VehicleData, CommandError> ValidateVehicle(VehicleData data)
    {
        var plate = LicensePlate.Create(data.Plate);
        if (plate.IsFailure) return plate.Error!;
        var model = (data.Model ?? string.Empty).Trim();
        if (model.Length is < 2 or > 100) return CommandError.Validation("vehicle-model-required");
        if (data.MaxLoadKg is <= 0 or > MaxLoadKg) return CommandError.Validation("capacity-weight-invalid");
        if (data.CargoLengthM is <= 0 or > MaxDimensionM || data.CargoWidthM is <= 0 or > MaxDimensionM ||
            data.CargoHeightM is <= 0 or > MaxDimensionM)
            return CommandError.Validation("cargo-dimensions-invalid");
        var volume = data.CargoVolumeM3 is > 0
            ? data.CargoVolumeM3.Value
            : Math.Round(data.CargoLengthM * data.CargoWidthM * data.CargoHeightM, 2);
        if (volume is <= 0 or > 99999) return CommandError.Validation("capacity-volume-invalid");
        var photo = string.IsNullOrWhiteSpace(data.PhotoS3Key) ? null : data.PhotoS3Key.Trim();
        return data with { Plate = plate.Value!.Value, Model = model, CargoVolumeM3 = volume, PhotoS3Key = photo };
    }

    private static Result<DriverData, CommandError> ValidateDriver(DriverData data)
    {
        var fullName = (data.FullName ?? string.Empty).Trim();
        if (fullName.Length is < 3 or > 200) return CommandError.Validation("full-name-required");
        if (data.ExperienceStars is < 1 or > 5) return CommandError.Validation("experience-stars-invalid");
        return data with { FullName = fullName };
    }
}
