using TrazzaPlatform.API.Iam.Application.Internal.OutboundServices;
using TrazzaPlatform.API.Iam.Domain.Model.Aggregates;
using TrazzaPlatform.API.Iam.Domain.Repositories;
using TrazzaPlatform.API.Iam.Domain.Services;

namespace TrazzaPlatform.API.Iam.Application.Internal.QueryServices;

public class UserQueryService(
    IUserRepository userRepository,
    IPersonIdentityRepository identityRepository,
    IIdentityVerificationRepository verificationRepository,
    ICarrierProfileRepository carrierProfileRepository,
    IMerchantProfileRepository merchantProfileRepository,
    IPersonalDataProtector dataProtector) : IUserQueryService
{
    public async Task<UserAccount?> GetAccountAsync(ulong userId, CancellationToken ct = default)
    {
        var user = await userRepository.FindWithRolesAsync(userId, ct);
        if (user is null) return null;
        var identity = await identityRepository.FindByUserIdAsync(userId, ct);
        var phone = identity is null ? null : dataProtector.Decrypt(identity.PhoneEncrypted);
        var carrier = await carrierProfileRepository.FindByUserIdAsync(userId, ct);
        var merchant = await merchantProfileRepository.FindByUserIdAsync(userId, ct);
        return new UserAccount(user, identity, phone, carrier, merchant);
    }

    public Task<List<IdentityVerification>> ListVerificationsAsync(ulong userId, CancellationToken ct = default) =>
        verificationRepository.ListByUserAsync(userId, ct);
}
