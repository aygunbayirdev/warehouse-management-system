using MediatR;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Application.Messaging;

public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent;
