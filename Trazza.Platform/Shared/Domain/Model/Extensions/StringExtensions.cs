using System;
using System.Text;

namespace Trazza.Platform.Shared.Domain.Model.Extensions;

/// <summary>
///     String helpers shared by every bounded context.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Converts a PascalCase or camelCase text into snake_case.
    ///     Examples: PaymentTransaction to payment_transaction, InTransit to in_transit, IoTDevice to io_t_device.
    /// </summary>
    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var builder = new StringBuilder(text.Length + 8);
        for (var index = 0; index < text.Length; index++)
        {
            var current = text[index];
            if (current is ' ' or '-')
            {
                if (builder.Length > 0 && builder[^1] != '_') builder.Append('_');
                continue;
            }

            if (char.IsUpper(current))
            {
                var previous = index > 0 ? text[index - 1] : '\0';
                var next = index + 1 < text.Length ? text[index + 1] : '\0';
                var startsWord = index > 0 &&
                                 (char.IsLower(previous) || char.IsDigit(previous) ||
                                  (char.IsUpper(previous) && char.IsLower(next)));
                if (startsWord && builder.Length > 0 && builder[^1] != '_') builder.Append('_');
                builder.Append(char.ToLowerInvariant(current));
                continue;
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}
