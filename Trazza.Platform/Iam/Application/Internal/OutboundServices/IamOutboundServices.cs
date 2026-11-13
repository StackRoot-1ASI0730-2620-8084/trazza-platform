using Trazza.Platform.Iam.Domain.Model.Aggregates;

namespace Trazza.Platform.Iam.Application.Internal.OutboundServices;

public interface IHashingService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

    Guid? ValidateAccessToken(string token);

    (string Token, byte[] Hash, DateTime ExpiresAt) GenerateRefreshToken();

    byte[] HashRefreshToken(string token);
}

public interface IPersonalDataProtector
{
    byte[] Encrypt(string plainText);

    string Decrypt(byte[] cipher);

    byte[] HashDni(string dni);
}

public interface IIdentityVerificationProvider
{
    Task<(bool Approved, string Reference, string? Reason)> VerifyAsync(string dni, string fullName,
        CancellationToken cancellationToken = default);
}
