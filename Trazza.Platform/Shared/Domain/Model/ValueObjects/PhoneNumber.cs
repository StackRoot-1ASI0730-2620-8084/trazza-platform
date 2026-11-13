using System.Text.RegularExpressions;

namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

/// <summary>
///     Peruvian mobile number: 9 digits starting with 9 (an optional +51 prefix is removed).
/// </summary>
public partial record PhoneNumber(string Value)
{
    public static Result<PhoneNumber, CommandError> Create(string? value)
    {
        var digits = NonDigits().Replace(value ?? string.Empty, string.Empty);
        if (digits.Length == 11 && digits.StartsWith("519")) digits = digits[2..];
        return MobilePattern().IsMatch(digits)
            ? new PhoneNumber(digits)
            : CommandError.Validation("phone-invalid");
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigits();

    [GeneratedRegex(@"^9\d{8}$")]
    private static partial Regex MobilePattern();
}