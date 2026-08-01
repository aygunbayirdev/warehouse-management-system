using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Captures domain events recorded on tracked <see cref="BaseEntity"/> instances as <see cref="OutboxMessage"/>
/// rows on the SAME DbContext, inside the pre-commit SavingChanges hook — so the outbox insert is part of
/// the exact same transaction as the aggregate write it originated from. A process crash after commit can
/// no longer lose the event: it is durably on disk, and <see cref="OutboxProcessor{TDbContext}"/> delivers
/// it at-least-once. Register once per module DbContext via <c>DbContextOptionsBuilder.AddInterceptors</c>.
/// </summary>
public sealed class OutboxWritingInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            WriteOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            WriteOutboxMessages(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void WriteOutboxMessages(DbContext context)
    {
        var entitiesWithEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                context.Set<OutboxMessage>().Add(OutboxMessage.FromDomainEvent(domainEvent));
            }

            entity.ClearDomainEvents();
        }
    }
}
