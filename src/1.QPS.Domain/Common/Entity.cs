using System;
using System.Collections.Generic;

namespace QPS.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    private readonly List<DomainEvent> _domainEvents = new();

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public IReadOnlyCollection<DomainEvent> GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class DomainEvent
{
    public DateTime OccurredOn { get; }

    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }
}