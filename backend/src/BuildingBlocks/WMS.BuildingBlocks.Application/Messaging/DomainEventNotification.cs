using MediatR;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Messaging;

/// <summary>
/// Wraps a domain-layer <see cref="IDomainEvent"/> so it can be published through MediatR
/// without the Domain/SharedKernel layer taking a dependency on MediatR.
/// </summary>
public sealed class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
