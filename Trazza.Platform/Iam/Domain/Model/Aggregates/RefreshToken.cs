namespace TrazzaPlatform.API.Iam.Domain.Model.Aggregates;

/// <summary>
///     Refresh token of a session. Only the SHA-256 of the token is stored (<c>refresh_tokens</c>).
/// </summary>
public class RefreshToken
{
    protected RefreshToken()
    {
        TokenHash = [];
    }

    public RefreshToken(ulong userId, byte[] tokenHash, DateTime expiresAt, DateTime now)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = now;
    }

    public ulong Id { get; private set; }
    public ulong UserId { get; private set; }
    public byte[] TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTime now) => RevokedAt ??= now;
}
