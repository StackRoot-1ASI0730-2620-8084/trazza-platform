using System.ComponentModel.DataAnnotations.Schema;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;

/// <summary>
///     Contract for aggregates that record domain events.
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

/// <summary>
///     Base class for aggregates. It keeps the domain events raised by the aggregate
///     until they are dispatched after a successful commit.
/// </summary>
public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IEvent> _domainEvents = [];

    [NotMapped]
    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void AddDomainEvent(IEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}