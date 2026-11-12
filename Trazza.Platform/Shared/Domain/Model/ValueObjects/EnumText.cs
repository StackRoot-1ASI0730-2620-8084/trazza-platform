using System.Text;

namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

/// <summary>
///     Converts enum members to the UPPER_SNAKE_CASE text used by the database ENUM columns and the API
///     (for example <c>TripStatus.PickedUp</c> is <c>PICKED_UP</c>) and back.
/// </summary>
public static class EnumText
{
    public static string ToText<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var name = value.ToString();
        var builder = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i])) builder.Append('_');
            builder.Append(char.ToUpperInvariant(name[i]));
        }

        return builder.ToString();
    }

    public static TEnum FromText<TEnum>(string text) where TEnum : struct, Enum =>
        Enum.Parse<TEnum>(text.Replace("_", string.Empty), true);

    public static bool TryParse<TEnum>(string? text, out TEnum value) where TEnum : struct, Enum
    {
        value = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var compact = text.Trim().Replace("_", string.Empty);
        return Enum.TryParse(compact, true, out value) && Enum.IsDefined(value);
    }
}
