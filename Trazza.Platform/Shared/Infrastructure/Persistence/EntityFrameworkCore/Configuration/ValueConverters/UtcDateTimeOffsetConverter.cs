using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;

/// <summary>
///     Stores DateTimeOffset values as UTC DATETIME columns and reads them back as UTC.
/// </summary>
public class UtcDateTimeOffsetConverter() : ValueConverter<DateTimeOffset, DateTime>(
    value => value.UtcDateTime,
    value => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc)));
