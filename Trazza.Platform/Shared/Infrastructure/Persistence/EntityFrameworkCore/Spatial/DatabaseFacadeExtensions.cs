using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Spatial;

/// <summary>
///     Raw SQL helpers for the tables that have <c>POINT SRID 4326</c> columns, which EF Core does not map.
/// </summary>
public static class DatabaseFacadeExtensions
{
    /// <summary>
    ///     Runs an <c>INSERT</c> and returns the <c>AUTO_INCREMENT</c> id it generated. The connection is kept open
    ///     between both statements so that <c>LAST_INSERT_ID()</c> belongs to the same session.
    /// </summary>
    public static async Task<ulong> InsertAndGetIdAsync(this DatabaseFacade database, FormattableString insertSql,
        CancellationToken cancellationToken = default)
    {
        await database.OpenConnectionAsync(cancellationToken);
        try
        {
            await database.ExecuteSqlInterpolatedAsync(insertSql, cancellationToken);
            var ids = await database.SqlQueryRaw<ulong>("SELECT LAST_INSERT_ID() AS Value").ToListAsync(cancellationToken);
            return ids[0];
        }
        finally
        {
            await database.CloseConnectionAsync();
        }
    }

    /// <summary>
    ///     Locks a row until the current transaction ends (<c>SELECT ... FOR UPDATE</c>).
    ///     <paramref name="table" /> must be a constant table name, never user input.
    /// </summary>
    public static Task LockRowAsync(this DatabaseFacade database, string table, ulong id,
        CancellationToken cancellationToken = default) =>
        database.ExecuteSqlRawAsync(string.Concat("SELECT id FROM ", table, " WHERE id = ", id.ToString(), " FOR UPDATE"),
            cancellationToken);

    /// <summary>
    ///     Comma separated list of numeric ids for an <c>IN (...)</c> clause. Ids are numbers, so this is injection safe.
    /// </summary>
    public static string ToSqlIdList(this IEnumerable<ulong> ids)
    {
        var list = string.Join(",", ids.Distinct());
        return list.Length == 0 ? "0" : list;
    }
}
