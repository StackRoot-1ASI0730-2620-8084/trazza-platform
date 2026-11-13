namespace Trazza.Platform.Iam.Domain.Model.ValueObjects;

public enum UserRoleType
{
    Carrier,
    Merchant
}

public enum KycStatus
{
    Pending,
    Validated,
    Rejected
}

public enum VerificationStatus
{
    Submitted,
    Validated,
    Rejected
}

public enum DriverSex
{
    F,
    M,
    X
}

public static class Roles
{
    public const string Carrier = "CARRIER";
    public const string Merchant = "MERCHANT";
}
