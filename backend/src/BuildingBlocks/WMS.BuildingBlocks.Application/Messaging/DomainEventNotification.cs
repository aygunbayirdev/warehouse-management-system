using MediatR;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Messaging;

/// <summary>
/// Wraps a domain-layer <see cref="IDomainEvent"/> so it can be published through MediatR
/// without the Domain/SharedKernel layer taking a dependency on MediatR. <paramref name="outboxMessageId"/>
/// is the id of the outbox row this event was delivered from — handlers that trigger another module's
/// command thread it through as an idempotency key (see Inventory's ProcessedDomainEvent ledger), since
/// the outbox relay guarantees at-least-once, not exactly-once, delivery.
/// </summary>
public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent, Guid outboxMessageId) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;

    public Guid OutboxMessageId { get; } = outboxMessageId;
}
