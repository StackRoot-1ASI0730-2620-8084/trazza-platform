using System.Text.RegularExpressions;
using Trazza.Platform.Shared.Domain.Model;

namespace Trazza.Platform.Iam.Domain.Model.ValueObjects;

public partial record Dni(string Value)
{
    public static Result<Dni, CommandError> Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return Pattern().IsMatch(normalized) ? new Dni(normalized) : CommandError.Validation("dni-invalid");
    }

    public string LastThree => Value[^3..];

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex Pattern();
}

public partial record Ruc(string Value)
{
    public static Result<Ruc, CommandError> Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim();
        return Pattern().IsMatch(normalized) ? new Ruc(normalized) : CommandError.Validation("ruc-invalid");
    }

    [GeneratedRegex(@"^(10|20)\d{9}$")]
    private static partial Regex Pattern();
}

public partial record EmailAddress(string Value)
{
    public static Result<EmailAddress, CommandError> Create(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
        return normalized.Length <= 255 && Pattern().IsMatch(normalized)
            ? new EmailAddress(normalized)
            : CommandError.Validation("email-invalid");
    }

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]{2,}$")]
    private static partial Regex Pattern();
}

public partial record LicensePlate(string Value)
{
    public static Result<LicensePlate, CommandError> Create(string? value)
    {
        var compact = NonAlphanumeric().Replace((value ?? string.Empty).ToUpperInvariant(), string.Empty);
        if (compact.Length != 6) return CommandError.Validation("plate-invalid");
        return new LicensePlate($"{compact[..3]}-{compact[3..]}");
    }

    [GeneratedRegex("[^A-Z0-9]")]
    private static partial Regex NonAlphanumeric();
}
