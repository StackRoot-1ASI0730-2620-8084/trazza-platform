namespace Trazza.Platform.Iam.Infrastructure.Tokens;

/// <summary>
///     JWT settings. The secret supports <c>%VARIABLE%</c> placeholders and must have at least 32 characters.
/// </summary>
public class TokenSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "trazza-platform";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}
