using Trazza.Platform.Iam.Domain.Model.ValueObjects;

namespace Trazza.Platform.Iam.Domain.Model.Commands;

/// <summary>Business data required for the merchant role.</summary>
public record MerchantData(string BusinessName, string Ruc, string BusinessType);

/// <summary>Registers an account with its person identity and one or both roles.</summary>
public record SignUpCommand(
    string Email,
    string Password,
    string FullName,
    string Dni,
    string Phone,
    IReadOnlyList<UserRoleType> Roles,
    MerchantData? Merchant);

public record SignInCommand(string Email, string Password);

public record RefreshSessionCommand(string RefreshToken);

public record SignOutCommand(ulong UserId, string RefreshToken);

public record UpdateIdentityCommand(ulong UserId, string FullName, string Phone);

public record AddRoleCommand(ulong UserId, UserRoleType Role, MerchantData? Merchant);

public record UpdateMerchantProfileCommand(ulong UserId, string BusinessName, string BusinessType);

public record SubmitIdentityVerificationCommand(ulong UserId, string? DocumentFrontS3Key, string? DocumentBackS3Key);
