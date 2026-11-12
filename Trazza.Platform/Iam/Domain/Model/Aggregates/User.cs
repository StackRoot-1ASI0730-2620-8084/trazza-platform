using Trazza.Platform.Iam.Domain.Model.ValueObjects;
using Trazza.Platform.Shared.Domain.Model.Entities;

namespace Trazza.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     Account aggregate root (<c>users</c> + <c>user_roles</c>). Only the password hash is stored.
/// </summary>
public class User : IAuditableEntity
{
    public const int MaxFailedLogins = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly List<UserRole> _roles = [];

    protected User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(string email, string passwordHash)
    {
        PublicId = Guid.CreateVersion7();
        Email = email;
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public ulong Id { get; private set; }
    public Guid PublicId { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public ushort FailedLoginCount { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    DateTimeOffset? IAuditableEntity.CreatedAt { get => CreatedAt; set => CreatedAt = value?.UtcDateTime ?? default; }
    DateTimeOffset? IAuditableEntity.UpdatedAt { get => UpdatedAt; set => UpdatedAt = value?.UtcDateTime ?? default; }
    public DateTime? DeletedAt { get; private set; }

    public IReadOnlyCollection<UserRole> Roles => _roles;

    public bool HasRole(UserRoleType role) => _roles.Any(userRole => userRole.Role == role);

    public bool GrantRole(UserRoleType role, DateTime now)
    {
        if (HasRole(role)) return false;
        _roles.Add(new UserRole(role, now));
        return true;
    }

    public bool CanSignIn => IsActive && DeletedAt is null;

    public bool IsLocked(DateTime now) => LockedUntil.HasValue && LockedUntil.Value > now;

    public void RegisterFailedLogin(DateTime now)
    {
        FailedLoginCount++;
        if (FailedLoginCount < MaxFailedLogins) return;
        LockedUntil = now.Add(LockoutDuration);
        FailedLoginCount = 0;
    }

    public void RegisterSuccessfulLogin(DateTime now)
    {
        FailedLoginCount = 0;
        LockedUntil = null;
        LastLoginAt = now;
    }
}

/// <summary>
///     Role granted to an account (<c>user_roles</c>, composite key user_id + role).
/// </summary>
public class UserRole
{
    protected UserRole()
    {
    }

    public UserRole(UserRoleType role, DateTime grantedAt)
    {
        Role = role;
        GrantedAt = grantedAt;
    }

    public ulong UserId { get; private set; }
    public UserRoleType Role { get; private set; }
    public DateTime GrantedAt { get; private set; }
}
