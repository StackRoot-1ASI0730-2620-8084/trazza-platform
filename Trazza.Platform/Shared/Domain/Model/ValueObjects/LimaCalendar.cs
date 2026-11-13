using System;

namespace Trazza.Platform.Shared.Domain.Model.ValueObjects;

public static class LimaCalendar
{
    public static readonly TimeSpan UtcOffset = TimeSpan.FromHours(-5);

    public static DateTimeOffset Now => DateTimeOffset.UtcNow.ToOffset(UtcOffset);

    public static DateOnly Today => DateOnly.FromDateTime(Now.DateTime);

    public static DateOnly ToLimaDate(DateTimeOffset moment) => DateOnly.FromDateTime(moment.ToOffset(UtcOffset).DateTime);
}
