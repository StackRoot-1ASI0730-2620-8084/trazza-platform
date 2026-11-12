using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cortex.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;

/// <summary>
///     Publishes the domain events recorded by the aggregates after the changes were saved successfully.
/// </summary>
public class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    private static readonly MethodInfo PublishMethod = typeof(IMediator).GetMethods()
        .Single(method => method.Name == nameof(IMediator.PublishAsync) && method.IsGenericMethodDefinition);

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        DispatchDomainEventsAsync(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (context is null) return;

        var aggregates = context.ChangeTracker.Entries<IAggregateRoot>()
            .Select(entry => entry.Entity)
            .Where(aggregate => aggregate.DomainEvents.Count != 0)
            .ToList();

        var domainEvents = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
        aggregates.ForEach(aggregate => aggregate.ClearDomainEvents());

        foreach (var domainEvent in domainEvents) await PublishAsync(domainEvent, cancellationToken);
    }

    /// <summary>
    ///     Publishes the event with its concrete type, so Cortex.Mediator finds the INotificationHandler of that event.
    /// </summary>
    private Task PublishAsync(IEvent domainEvent, CancellationToken cancellationToken)
    {
        return (Task)PublishMethod.MakeGenericMethod(domainEvent.GetType())
            .Invoke(mediator, [domainEvent, cancellationToken])!;
    }
}
