namespace TrazzaPlatform.API.Iam.Domain.Model.ValueObjects;

/// <summary>Role of an account. One account can be carrier and merchant at the same time (<c>user_roles</c>).</summary>
public enum UserRoleType
{
    Carrier,
    Merchant
}

/// <summary>Identity validation state of a person (<c>person_identities.kyc_status</c>).</summary>
public enum KycStatus
{
    Pending,
    Validated,
    Rejected
}

/// <summary>State of one identity verification attempt (<c>identity_verifications.status</c>).</summary>
public enum VerificationStatus
{
    Submitted,
    Validated,
    Rejected
}

/// <summary>Optional sex of a fleet driver (<c>drivers.sex</c>).</summary>
public enum DriverSex
{
    F,
    M,
    X
}

/// <summary>Role names used by the <c>Authorize</c> attribute.</summary>
public static class Roles
{
    public const string Carrier = "CARRIER";
    public const string Merchant = "MERCHANT";
}
