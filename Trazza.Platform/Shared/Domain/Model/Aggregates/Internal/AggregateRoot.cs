using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Trazza.Platform.Shared.Domain.Model.Events;

namespace Trazza.Platform.Shared.Domain.Model.Aggregates.Internal;

public interface IAggregateRoot
{
    IReadOnlyCollection<IEvent> DomainEvents { get; }

    void ClearDomainEvents();
}

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