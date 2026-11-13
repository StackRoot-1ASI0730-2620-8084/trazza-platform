using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Iam.Domain.Model.Commands;
using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Iam.Domain.Services;
using Trazza.Platform.Iam.Interfaces.Rest.Resources;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Iam.Interfaces.Rest.Transform;

public static class IamResourceAssembler
{
    public static Result<SignUpCommand, CommandError> ToCommand(SignUpResource resource)
    {
        var roles = new List<UserRoleType>();
        foreach (var text in resource.Roles ?? [])
        {
            if (!EnumText.TryParse<UserRoleType>(text, out var role)) return CommandError.Validation("role-invalid");
            roles.Add(role);
        }

        return new SignUpCommand(resource.Email, resource.Password, resource.FullName, resource.Dni, resource.Phone,
            roles, ToMerchantData(resource.Merchant));
    }

    public static Result<AddRoleCommand, CommandError> ToCommand(ulong userId, AddRoleResource resource)
    {
        if (!EnumText.TryParse<UserRoleType>(resource.Role, out var role)) return CommandError.Validation("role-invalid");
        return new AddRoleCommand(userId, role, ToMerchantData(resource.Merchant));
    }

    public static Result<DriverData, CommandError> ToDriverData(SaveDriverResource resource)
    {
        DriverSex? sex = null;
        if (!string.IsNullOrWhiteSpace(resource.Sex))
        {
            if (!EnumText.TryParse<DriverSex>(resource.Sex, out var parsed)) return CommandError.Validation("sex-invalid");
            sex = parsed;
        }

        return new DriverData(resource.FullName, sex, resource.ExperienceStars, resource.IsActive ?? true);
    }

    public static VehicleData ToVehicleData(SaveVehicleResource resource) =>
        new(resource.Plate, resource.Model, resource.MaxLoadKg, resource.CargoLengthM, resource.CargoWidthM,
            resource.CargoHeightM, resource.CargoVolumeM3, resource.PhotoS3Key, resource.IsActive ?? true);

    public static AccountResource ToResource(UserAccount account) =>
        new(account.User.PublicId,
            account.User.Email,
            account.User.Roles.Select(role => EnumText.ToText(role.Role)).OrderBy(role => role).ToList(),
            account.Identity is null
                ? null
                : new IdentityResource(account.Identity.FullName, $"*****{account.Identity.DniLast3}", account.Phone,
                    EnumText.ToText(account.Identity.KycStatus)),
            account.CarrierProfile is null ? null : new CarrierProfileResource(account.CarrierProfile.Id),
            account.MerchantProfile is null
                ? null
                : new MerchantProfileResource(account.MerchantProfile.Id, account.MerchantProfile.BusinessName,
                    account.MerchantProfile.Ruc, account.MerchantProfile.BusinessType),
            account.User.CreatedAt);

    public static AuthenticatedSessionResource ToResource(AuthenticatedSession session) =>
        new(session.AccessToken, session.AccessTokenExpiresAt, session.RefreshToken, session.RefreshTokenExpiresAt,
            ToResource(session.Account));

    public static IdentityVerificationResource ToResource(IdentityVerification verification) =>
        new(verification.Id, verification.Provider, EnumText.ToText(verification.Status),
            verification.ExternalReference, verification.RejectionReason, verification.SubmittedAt,
            verification.ReviewedAt);

    public static VehicleResource ToResource(Vehicle vehicle) =>
        new(vehicle.PublicId, vehicle.Plate, vehicle.Model, vehicle.MaxLoadKg, vehicle.CargoLengthM, vehicle.CargoWidthM,
            vehicle.CargoHeightM, vehicle.CargoVolumeM3, vehicle.PhotoS3Key, vehicle.IsActive);

    public static DriverResource ToResource(Driver driver) =>
        new(driver.Id, driver.FullName, driver.Sex.HasValue ? EnumText.ToText(driver.Sex.Value) : null,
            driver.ExperienceStars, driver.IsActive);

    public static SavedAddressResource ToResource(SavedAddress address) =>
        new(address.Id, address.Label, address.Address, address.District, address.Latitude, address.Longitude);

    private static MerchantData? ToMerchantData(MerchantResource? resource) =>
        resource is null ? null : new MerchantData(resource.BusinessName, resource.Ruc, resource.BusinessType);
}
