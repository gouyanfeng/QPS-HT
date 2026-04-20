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

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Entity))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return Id == ((Entity)obj).Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
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