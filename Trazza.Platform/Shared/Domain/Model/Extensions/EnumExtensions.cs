using System;

namespace Trazza.Platform.Shared.Domain.Model.Extensions;

public static class EnumExtensions
{
    public static string ToSnakeCaseValue(this Enum value)
    {
        return value.ToString().ToSnakeCase();
    }

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
