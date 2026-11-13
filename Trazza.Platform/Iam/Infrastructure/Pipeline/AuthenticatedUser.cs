using Trazza.Platform.Iam.Domain.Model.ValueObjects;

namespace Trazza.Platform.Iam.Infrastructure.Pipeline;

public record AuthenticatedUser(ulong Id, Guid PublicId, IReadOnlyCollection<UserRoleType> Roles)
{
    public bool IsCarrier => Roles.Contains(UserRoleType.Carrier);
    public bool IsMerchant => Roles.Contains(UserRoleType.Merchant);
}

public static class HttpContextAuthenticationExtensions
{
    public const string ItemKey = "Trazza.AuthenticatedUser";

    public static AuthenticatedUser GetAuthenticatedUser(this HttpContext context) =>
        context.Items[ItemKey] as AuthenticatedUser
        ?? throw new InvalidOperationException("The request has no authenticated user.");

    public static AuthenticatedUser? FindAuthenticatedUser(this HttpContext context) =>
        context.Items[ItemKey] as AuthenticatedUser;
}
