using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Trazza.Platform.Iam.Application.Internal.OutboundServices;
using Trazza.Platform.Iam.Domain.Model.Aggregates;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Iam.Infrastructure.Tokens;

public class JwtTokenService(IOptions<TokenSettings> options, TimeProvider timeProvider) : ITokenService
{
    private readonly TokenSettings _settings = options.Value;

    private SymmetricSecurityKey SigningKey => new(Encoding.UTF8.GetBytes(_settings.Secret));

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(_settings.AccessTokenMinutes);
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, user.PublicId.ToString()) };
        claims.AddRange(user.Roles.Select(role => new Claim("role", EnumText.ToText(role.Role))));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now,
            Expires = expiresAt,
            IssuedAt = now,
            Issuer = _settings.Issuer,
            Audience = _settings.Issuer,
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256)
        };
        var handler = new JwtSecurityTokenHandler();
        return (handler.WriteToken(handler.CreateToken(descriptor)), expiresAt);
    }

    public Guid? ValidateAccessToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SigningKey,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Issuer,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out _);
            var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(subject, out var publicId) ? publicId : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public (string Token, byte[] Hash, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(48));
        var expiresAt = timeProvider.GetUtcNow().UtcDateTime.AddDays(_settings.RefreshTokenDays);
        return (token, HashRefreshToken(token), expiresAt);
    }

    public byte[] HashRefreshToken(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));
}
