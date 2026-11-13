using System;
using System.Threading;
using System.Threading.Tasks;
using Trazza.Platform.Shared.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Saves every change tracked by the context in a single transaction.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);

        // A repository may open a transaction to hold a row lock or to read LAST_INSERT_ID on the same connection.
        if (context.Database.CurrentTransaction is { } transaction)
            await transaction.CommitAsync(cancellationToken);
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> work,
        Func<TResult, bool> isSuccess, CancellationToken cancellationToken = default)
    {
        if (context.Database.CurrentTransaction is not null) return await work();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await work();
            if (isSuccess(result)) await transaction.CommitAsync(cancellationToken);
            else await transaction.RollbackAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
