using System.Threading;
using System.Threading.Tasks;

namespace Trazza.Platform.Shared.Domain.Repositories;

/// <summary>
///     Commits all the changes tracked during a request in a single database transaction.
/// </summary>
public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
