using System;

namespace Trazza.Platform.Shared.Domain.Model.Extensions;

/// <summary>
///     Enum helpers to exchange enumerated values with the web application in snake_case.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    ///     Returns the snake_case text of an enum value. Example: EShipmentStatus.InTransit to in_transit.
    /// </summary>
    public static string ToSnakeCaseValue(this Enum value)
    {
        return value.ToString().ToSnakeCase();
    }

    /// <summary>
    ///     Parses a snake_case text into its enum value. Returns false when the text does not match any value.
    /// </summary>
    public static bool TryParseSnakeCase<TEnum>(this string? text, out TEnum value) where TEnum : struct, Enum
    {
        foreach (var candidate in Enum.GetValues<TEnum>())
        {
            if (!string.Equals(candidate.ToSnakeCaseValue(), text?.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            value = candidate;
            return true;
        }

        value = default;
        return false;
    }
}
