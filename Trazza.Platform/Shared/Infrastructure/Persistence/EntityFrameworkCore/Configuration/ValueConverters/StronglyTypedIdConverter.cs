using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;

/// <summary>
///     Converts a strongly-typed identifier to its primitive database value and back.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier.</typeparam>
/// <typeparam name="TValue">The primitive value stored in the database.</typeparam>
public class StronglyTypedIdConverter<TId, TValue>(
    Expression<Func<TId, TValue>> toProvider,
    Expression<Func<TValue, TId>> fromProvider)
    : ValueConverter<TId, TValue>(toProvider, fromProvider)
    where TId : struct
    where TValue : struct;
