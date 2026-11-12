using Trazza.Platform.Iam.Domain.Model.Aggregates;

namespace Trazza.Platform.Iam.Application.Internal.OutboundServices;

/// <summary>Password hashing (BCrypt). The password itself is never stored.</summary>
public interface IHashingService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}

/// <summary>Access token (JWT) and opaque refresh token generation.</summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

    Guid? ValidateAccessToken(string token);

    (string Token, byte[] Hash, DateTime ExpiresAt) GenerateRefreshToken();

    byte[] HashRefreshToken(string token);
}

/// <summary>Application-level protection of personal data (AES-256-GCM and HMAC-SHA256).</summary>
public interface IPersonalDataProtector
{
    byte[] Encrypt(string plainText);

    string Decrypt(byte[] cipher);

    byte[] HashDni(string dni);
}

/// <summary>Identity provider (RENIEC). The MVP uses a simulated provider.</summary>
public interface IIdentityVerificationProvider
{
    Task<(bool Approved, string Reference, string? Reason)> VerifyAsync(string dni, string fullName,
        CancellationToken cancellationToken = default);
}
