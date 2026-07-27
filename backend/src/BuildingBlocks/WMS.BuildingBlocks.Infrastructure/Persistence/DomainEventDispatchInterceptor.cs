using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Collects domain events recorded on tracked <see cref="BaseEntity"/> instances and publishes them
/// through MediatR only after SaveChanges has committed successfully, so a handler failure can never
/// leave the write side and the reacting side out of sync with a half-committed transaction.
/// Register once per module DbContext via <c>DbContextOptionsBuilder.AddInterceptors</c>.
/// </summary>
public sealed class DomainEventDispatchInterceptor(IPublisher publisher) : SaveChangesInterceptor
{
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;

            await publisher.Publish(notification, cancellationToken);
        }
    }
}
