using Trazza.Platform.Iam.Application.Internal.OutboundServices;
using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Iam.Domain.Model.Commands;
using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Iam.Domain.Repositories;
using Trazza.Platform.Iam.Domain.Services;
using Trazza.Platform.Shared.Domain.Model;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Repositories;

namespace Trazza.Platform.Iam.Application.Internal.CommandServices;

public class UserCommandService(
    IUserRepository userRepository,
    IPersonIdentityRepository identityRepository,
    IIdentityVerificationRepository verificationRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ICarrierProfileRepository carrierProfileRepository,
    IMerchantProfileRepository merchantProfileRepository,
    IUserQueryService userQueryService,
    IHashingService hashingService,
    ITokenService tokenService,
    IPersonalDataProtector dataProtector,
    IIdentityVerificationProvider verificationProvider,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IUserCommandService
{
    public const int MinPasswordLength = 8;

    private DateTime Now => timeProvider.GetUtcNow().UtcDateTime;

    public async Task<Result<AuthenticatedSession, CommandError>> Handle(SignUpCommand command,
        CancellationToken ct = default)
    {
        var email = EmailAddress.Create(command.Email);
        if (email.IsFailure) return email.Error!;
        if ((command.Password ?? string.Empty).Length < MinPasswordLength)
            return CommandError.Validation("password-too-short");
        var fullName = NormalizeName(command.FullName);
        if (fullName is null) return CommandError.Validation("full-name-required");
        var dni = Dni.Create(command.Dni);
        if (dni.IsFailure) return dni.Error!;
        var phone = PhoneNumber.Create(command.Phone);
        if (phone.IsFailure) return phone.Error!;
        var roles = command.Roles.Distinct().ToList();
        if (roles.Count == 0) return CommandError.Validation("role-required");

        MerchantProfileData? merchant = null;
        if (roles.Contains(UserRoleType.Merchant))
        {
            var merchantResult = ValidateMerchant(command.Merchant);
            if (merchantResult.IsFailure) return merchantResult.Error!;
            merchant = merchantResult.Value;
        }

        if (await userRepository.ExistsByEmailAsync(email.Value!.Value, ct))
            return CommandError.Conflict("email-already-registered");
        var dniHash = dataProtector.HashDni(dni.Value!.Value);
        if (await identityRepository.ExistsByDniHashAsync(dniHash, ct))
            return CommandError.Conflict("dni-already-registered");
        if (merchant is not null && await merchantProfileRepository.ExistsByRucAsync(merchant.Ruc, ct))
            return CommandError.Conflict("ruc-already-registered");

        return await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var now = Now;
            var user = new User(email.Value!.Value, hashingService.HashPassword(command.Password!));
            foreach (var role in roles) user.GrantRole(role, now);
            await userRepository.AddAsync(user, ct);
            await unitOfWork.CompleteAsync(ct);

            await identityRepository.AddAsync(new PersonIdentity(user.Id, fullName!, dataProtector.Encrypt(dni.Value!.Value),
                dniHash, dni.Value!.LastThree, dataProtector.Encrypt(phone.Value!.Value)), ct);
            if (roles.Contains(UserRoleType.Carrier)) await carrierProfileRepository.AddAsync(new CarrierProfile(user.Id), ct);
            if (merchant is not null)
                await merchantProfileRepository.AddAsync(
                    new MerchantProfile(user.Id, merchant.BusinessName, merchant.Ruc, merchant.BusinessType), ct);
            await unitOfWork.CompleteAsync(ct);

            return await IssueSessionAsync(user, ct);
        }, result => result.IsSuccess, ct);
    }

    public async Task<Result<AuthenticatedSession, CommandError>> Handle(SignInCommand command,
        CancellationToken ct = default)
    {
        var email = EmailAddress.Create(command.Email);
        if (email.IsFailure || string.IsNullOrEmpty(command.Password))
            return CommandError.Unauthorized("invalid-credentials");

        var user = await userRepository.FindByEmailAsync(email.Value!.Value, ct);
        if (user is null || !user.CanSignIn) return CommandError.Unauthorized("invalid-credentials");
        if (user.IsLocked(Now)) return CommandError.Forbidden("account-locked");

        if (!hashingService.VerifyPassword(command.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(Now);
            await unitOfWork.CompleteAsync(ct);
            return CommandError.Unauthorized("invalid-credentials");
        }

        user.RegisterSuccessfulLogin(Now);
        return await IssueSessionAsync(user, ct);
    }

    public async Task<Result<AuthenticatedSession, CommandError>> Handle(RefreshSessionCommand command,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken)) return CommandError.Unauthorized("invalid-refresh-token");
        var stored = await refreshTokenRepository.FindByHashAsync(tokenService.HashRefreshToken(command.RefreshToken), ct);
        if (stored is null || !stored.IsActive(Now)) return CommandError.Unauthorized("invalid-refresh-token");

        var user = await userRepository.FindWithRolesAsync(stored.UserId, ct);
        if (user is null || !user.CanSignIn) return CommandError.Unauthorized("invalid-refresh-token");

        stored.Revoke(Now);
        return await IssueSessionAsync(user, ct);
    }

    public async Task<Result<bool, CommandError>> Handle(SignOutCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken)) return true;
        var stored = await refreshTokenRepository.FindByHashAsync(tokenService.HashRefreshToken(command.RefreshToken), ct);
        if (stored is null || stored.UserId != command.UserId) return true;
        stored.Revoke(Now);
        await unitOfWork.CompleteAsync(ct);
        return true;
    }

    public async Task<Result<UserAccount, CommandError>> Handle(UpdateIdentityCommand command,
        CancellationToken ct = default)
    {
        var fullName = NormalizeName(command.FullName);
        if (fullName is null) return CommandError.Validation("full-name-required");
        var phone = PhoneNumber.Create(command.Phone);
        if (phone.IsFailure) return phone.Error!;

        var identity = await identityRepository.FindByUserIdAsync(command.UserId, ct);
        if (identity is null) return CommandError.NotFound("identity-not-found");
        identity.UpdateContact(fullName!, dataProtector.Encrypt(phone.Value!.Value));
        await unitOfWork.CompleteAsync(ct);
        return await LoadAccountAsync(command.UserId, ct);
    }

    public async Task<Result<UserAccount, CommandError>> Handle(AddRoleCommand command, CancellationToken ct = default)
    {
        var user = await userRepository.FindWithRolesAsync(command.UserId, ct);
        if (user is null) return CommandError.NotFound("user-not-found");
        if (user.HasRole(command.Role)) return CommandError.Conflict("role-already-granted");

        if (command.Role == UserRoleType.Merchant)
        {
            var merchant = ValidateMerchant(command.Merchant);
            if (merchant.IsFailure) return merchant.Error!;
            if (await merchantProfileRepository.ExistsByRucAsync(merchant.Value!.Ruc, ct))
                return CommandError.Conflict("ruc-already-registered");
            if (await merchantProfileRepository.FindByUserIdAsync(user.Id, ct) is null)
                await merchantProfileRepository.AddAsync(new MerchantProfile(user.Id, merchant.Value.BusinessName,
                    merchant.Value.Ruc, merchant.Value.BusinessType), ct);
        }
        else if (await carrierProfileRepository.FindByUserIdAsync(user.Id, ct) is null)
        {
            await carrierProfileRepository.AddAsync(new CarrierProfile(user.Id), ct);
        }

        user.GrantRole(command.Role, Now);
        await unitOfWork.CompleteAsync(ct);
        return await LoadAccountAsync(user.Id, ct);
    }

    public async Task<Result<UserAccount, CommandError>> Handle(UpdateMerchantProfileCommand command,
        CancellationToken ct = default)
    {
        var businessName = (command.BusinessName ?? string.Empty).Trim();
        var businessType = (command.BusinessType ?? string.Empty).Trim();
        if (businessName.Length is < 2 or > 255) return CommandError.Validation("business-name-required");
        if (businessType.Length is < 2 or > 100) return CommandError.Validation("business-type-required");

        var profile = await merchantProfileRepository.FindByUserIdAsync(command.UserId, ct);
        if (profile is null) return CommandError.NotFound("merchant-profile-not-found");
        profile.Update(businessName, businessType);
        await unitOfWork.CompleteAsync(ct);
        return await LoadAccountAsync(command.UserId, ct);
    }

    public async Task<Result<IdentityVerification, CommandError>> Handle(SubmitIdentityVerificationCommand command,
        CancellationToken ct = default)
    {
        var identity = await identityRepository.FindByUserIdAsync(command.UserId, ct);
        if (identity is null) return CommandError.NotFound("identity-not-found");
        if (identity.KycStatus == KycStatus.Validated) return CommandError.Conflict("identity-already-validated");

        var verification = new IdentityVerification(command.UserId, Blank(command.DocumentFrontS3Key),
            Blank(command.DocumentBackS3Key), Now);
        var (approved, reference, reason) = await verificationProvider.VerifyAsync(
            dataProtector.Decrypt(identity.DniEncrypted), identity.FullName, ct);
        if (approved)
        {
            verification.Validate(reference, Now);
            identity.SetKycStatus(KycStatus.Validated);
        }
        else
        {
            verification.Reject(reason ?? "not-validated", Now);
            identity.SetKycStatus(KycStatus.Rejected);
        }

        await verificationRepository.AddAsync(verification, ct);
        await unitOfWork.CompleteAsync(ct);
        return verification;
    }

    private async Task<Result<AuthenticatedSession, CommandError>> IssueSessionAsync(User user, CancellationToken ct)
    {
        var (accessToken, accessExpiresAt) = tokenService.GenerateAccessToken(user);
        var (refreshToken, refreshHash, refreshExpiresAt) = tokenService.GenerateRefreshToken();
        await refreshTokenRepository.AddAsync(new RefreshToken(user.Id, refreshHash, refreshExpiresAt, Now), ct);
        await unitOfWork.CompleteAsync(ct);

        var account = await userQueryService.GetAccountAsync(user.Id, ct);
        if (account is null) return CommandError.NotFound("user-not-found");
        return new AuthenticatedSession(account, accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    private async Task<Result<UserAccount, CommandError>> LoadAccountAsync(ulong userId, CancellationToken ct)
    {
        var account = await userQueryService.GetAccountAsync(userId, ct);
        if (account is null) return CommandError.NotFound("user-not-found");
        return account;
    }

    private static Result<MerchantProfileData, CommandError> ValidateMerchant(MerchantData? data)
    {
        if (data is null) return CommandError.Validation("merchant-data-required");
        var businessName = (data.BusinessName ?? string.Empty).Trim();
        if (businessName.Length is < 2 or > 255) return CommandError.Validation("business-name-required");
        var businessType = (data.BusinessType ?? string.Empty).Trim();
        if (businessType.Length is < 2 or > 100) return CommandError.Validation("business-type-required");
        var ruc = Ruc.Create(data.Ruc);
        if (ruc.IsFailure) return ruc.Error!;
        return new MerchantProfileData(businessName, ruc.Value!.Value, businessType);
    }

    private static string? NormalizeName(string? fullName)
    {
        var normalized = (fullName ?? string.Empty).Trim();
        return normalized.Length is < 3 or > 200 ? null : normalized;
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record MerchantProfileData(string BusinessName, string Ruc, string BusinessType);
}
