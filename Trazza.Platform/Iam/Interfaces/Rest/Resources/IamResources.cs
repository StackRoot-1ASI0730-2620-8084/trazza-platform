namespace Trazza.Platform.Iam.Interfaces.Rest.Resources;

public record MerchantResource(string BusinessName, string Ruc, string BusinessType);

public record SignUpResource(
    string Email,
    string Password,
    string FullName,
    string Dni,
    string Phone,
    List<string> Roles,
    MerchantResource? Merchant);

public record SignInResource(string Email, string Password);

public record RefreshSessionResource(string RefreshToken);

public record IdentityResource(string FullName, string DniMasked, string? Phone, string KycStatus);

public record CarrierProfileResource(ulong Id);

public record MerchantProfileResource(ulong Id, string BusinessName, string Ruc, string BusinessType);

public record AccountResource(
    Guid Id,
    string Email,
    List<string> Roles,
    IdentityResource? Identity,
    CarrierProfileResource? CarrierProfile,
    MerchantProfileResource? MerchantProfile,
    DateTime CreatedAt);

public record AuthenticatedSessionResource(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AccountResource User);

public record UpdateIdentityResource(string FullName, string Phone);

public record AddRoleResource(string Role, MerchantResource? Merchant);

public record UpdateMerchantProfileResource(string BusinessName, string BusinessType);

public record SubmitIdentityVerificationResource(string? DocumentFrontS3Key, string? DocumentBackS3Key);

public record IdentityVerificationResource(
    ulong Id,
    string Provider,
    string Status,
    string? ExternalReference,
    string? RejectionReason,
    DateTime SubmittedAt,
    DateTime? ReviewedAt);

public record SaveVehicleResource(
    string Plate,
    string Model,
    decimal MaxLoadKg,
    decimal CargoLengthM,
    decimal CargoWidthM,
    decimal CargoHeightM,
    decimal? CargoVolumeM3,
    string? PhotoS3Key,
    bool? IsActive);

public record VehicleResource(
    Guid Id,
    string Plate,
    string Model,
    decimal MaxLoadKg,
    decimal CargoLengthM,
    decimal CargoWidthM,
    decimal CargoHeightM,
    decimal CargoVolumeM3,
    string? PhotoS3Key,
    bool IsActive);

public record SaveDriverResource(string FullName, string? Sex, int ExperienceStars, bool? IsActive);

public record DriverResource(ulong Id, string FullName, string? Sex, int ExperienceStars, bool IsActive);

public record CreateSavedAddressResource(string Label, string Address, string? District, double Latitude, double Longitude);

public record SavedAddressResource(
    ulong Id,
    string Label,
    string Address,
    string? District,
    double Latitude,
    double Longitude);
