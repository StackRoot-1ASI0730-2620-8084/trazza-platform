using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trazza.Platform.Shared.Domain.Model.Extensions;
using Trazza.Platform.Shared.Domain.Model.ValueObjects;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            if (!entity.IsOwned())
            {
                var tableName = entity.GetTableName();
                if (!string.IsNullOrEmpty(tableName))
                    entity.SetTableName(tableName == entity.GetDefaultTableName()
                        ? tableName.ToSnakeCase().Pluralize(inputIsKnownToBeSingular: false)
                        : tableName.ToSnakeCase());
            }

            foreach (var property in entity.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName)) property.SetColumnName(columnName.ToSnakeCase());
            }

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName)) key.SetName(keyName.ToSnakeCase());
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var constraintName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(constraintName)) foreignKey.SetConstraintName(constraintName.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName)) index.SetDatabaseName(indexName.ToSnakeCase());
            }
        }
    }

    public static PropertyBuilder<TEnum> HasEnumText<TEnum>(this PropertyBuilder<TEnum> property)
        where TEnum : struct, Enum =>
        property.HasConversion(value => EnumText.ToText(value), text => EnumText.FromText<TEnum>(text));

    public static PropertyBuilder<TEnum?> HasNullableEnumText<TEnum>(this PropertyBuilder<TEnum?> property)
        where TEnum : struct, Enum =>
        property.HasConversion(
            value => value.HasValue ? EnumText.ToText(value.Value) : null,
            text => text == null ? null : (TEnum?)EnumText.FromText<TEnum>(text));

    public static PropertyBuilder<Guid> HasPublicId(this PropertyBuilder<Guid> property) =>
        property.HasConversion(value => value.ToByteArray(), bytes => new Guid(bytes))
            .HasColumnType("binary(16)");
}
