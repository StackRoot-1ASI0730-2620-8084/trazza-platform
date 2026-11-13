using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trazza.Platform.Shared.Domain.Repositories;

/// <summary>
///     Commits all the changes tracked during a request in a single database transaction.
/// </summary>
public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Runs the work inside a database transaction. It commits when the work returns a success
    ///     and rolls back otherwise.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> work, Func<TResult, bool> isSuccess,
        CancellationToken cancellationToken = default);
}
