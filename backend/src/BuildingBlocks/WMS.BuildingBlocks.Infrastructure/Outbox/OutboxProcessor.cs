using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Polls one producing module's own outbox table and dispatches unprocessed rows through the shared
/// MediatR <see cref="IPublisher"/> — the same <see cref="DomainEventNotification{TDomainEvent}"/> wrapping
/// the old in-memory interceptor used, just triggered from here instead, so existing domain event handlers
/// need no rewrite. One instance per producing module's DbContext type; see Add{Module}Module for
/// registration. Single VPS, single backend container — no leader election/distributed locking needed.
/// </summary>
public sealed class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;
    private const int MaxRetryCount = 10;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollingInterval);
        do
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Outbox processor for {DbContext} failed unexpectedly; retrying next cycle", typeof(TDbContext).Name);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await dbContext.Set<OutboxMessage>()
            .Where(m => m.ProcessedAtUtc == null && m.RetryCount < MaxRetryCount)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var clrType = Type.GetType(message.Type)
                    ?? throw new InvalidOperationException($"Cannot resolve CLR type '{message.Type}'.");

                var domainEvent = (IDomainEvent)JsonSerializer.Deserialize(message.Payload, clrType)!;

                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(clrType);
                var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent, message.Id)!;

                await publisher.Publish(notification, cancellationToken);

                message.MarkProcessed(DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to process outbox message {OutboxMessageId} ({Type}), attempt {Attempt}",
                    message.Id, message.Type, message.RetryCount + 1);
                message.MarkFailed(ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
