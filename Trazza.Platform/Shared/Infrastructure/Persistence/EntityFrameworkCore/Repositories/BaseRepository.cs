using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trazza.Platform.Shared.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class BaseRepository<TEntity>(AppDbContext context) : IBaseRepository<TEntity> where TEntity : class
{
    protected AppDbContext Context { get; } = context;

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual async Task<TEntity?> FindByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : struct
    {
        return await Context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public virtual async Task<TEntity?> FindByIdAsNoTrackingAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : struct
    {
        return await Context.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(HasId(id), cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> ListAsNoTrackingAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
    }

    public void Update(TEntity entity)
    {
        Context.Set<TEntity>().Update(entity);
    }

    public void Remove(TEntity entity)
    {
        Context.Set<TEntity>().Remove(entity);
    }

    private Expression<Func<TEntity, bool>> HasId<TId>(TId id) where TId : struct
    {
        var keyName = Context.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Single().Name
                      ?? throw new InvalidOperationException($"{typeof(TEntity).Name} has no primary key.");

        var entity = Expression.Parameter(typeof(TEntity), "entity");
        var key = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(TId)], entity, Expression.Constant(keyName));
        Expression<Func<TId>> value = () => id;

        return Expression.Lambda<Func<TEntity, bool>>(Expression.Equal(key, value.Body), entity);
    }
}
