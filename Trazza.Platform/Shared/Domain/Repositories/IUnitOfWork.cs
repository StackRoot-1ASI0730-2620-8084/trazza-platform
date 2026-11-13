using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trazza.Platform.Shared.Domain.Repositories;

public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> work, Func<TResult, bool> isSuccess,
        CancellationToken cancellationToken = default);
}
