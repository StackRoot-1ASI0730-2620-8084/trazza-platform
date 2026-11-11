using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Trazza.Platform.Shared.Domain.Repositories;

/// <summary>
///     Base repository contract with the common operations of every aggregate.
///     Changes are persisted by <see cref="IUnitOfWork" />.
/// </summary>
/// <typeparam name="TEntity">The aggregate type.</typeparam>
public interface IBaseRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task<TEntity?> FindByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : struct;

    Task<TEntity?> FindByIdAsNoTrackingAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : struct;

    Task<IEnumerable<TEntity>> ListAsNoTrackingAsync(CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
