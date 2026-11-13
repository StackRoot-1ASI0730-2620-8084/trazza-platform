using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Json;

/// <summary>
///     Every date-time in the database is UTC; this converter writes it with the <c>Z</c> suffix and
///     turns incoming values into UTC.
/// </summary>
public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();
        if (string.IsNullOrWhiteSpace(text)) throw new JsonException("date-time-invalid");
        var parsed = DateTime.Parse(text, CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
        return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        writer.WriteStringValue(utc.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
    }
}
