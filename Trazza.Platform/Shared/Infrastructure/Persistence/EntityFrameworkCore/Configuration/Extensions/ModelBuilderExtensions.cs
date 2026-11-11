using Humanizer;
using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Shared.Domain.Model.Extensions;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Model builder helpers shared by every bounded context.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Applies the snake_case naming convention of trazza_database to tables, columns, keys,
    ///     foreign keys and indexes. Default table names are also pluralized (PersonIdentity to person_identities).
    ///     Names set explicitly with ToTable or HasColumnName are only converted to snake_case.
    /// </summary>
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
}
