using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.ValueConverters;

public class StronglyTypedIdConverter<TId, TValue>(
    Expression<Func<TId, TValue>> toProvider,
    Expression<Func<TValue, TId>> fromProvider)
    : ValueConverter<TId, TValue>(toProvider, fromProvider)
    where TId : struct
    where TValue : struct;
