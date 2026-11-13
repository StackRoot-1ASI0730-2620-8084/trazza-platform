namespace Trazza.Platform.Iam.Infrastructure.Tokens;

public class TokenSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "trazza-platform";
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 14;
}
