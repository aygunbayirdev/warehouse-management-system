using System.Text.Json;
using WMS.SharedKernel;

namespace WMS.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Durable record of a domain event, written in the SAME transaction as the aggregate change that
/// raised it (see <see cref="OutboxWritingInterceptor"/>), so a process crash between commit and
/// dispatch can no longer lose the event silently. <see cref="OutboxProcessor{TDbContext}"/> delivers
/// unprocessed rows at-least-once.
/// </summary>
public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = null!;

    public string Payload { get; private set; } = null!;

    public DateTimeOffset OccurredOnUtc { get; private set; }

    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    public string? Error { get; private set; }

    public int RetryCount { get; private set; }

    public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
    {
        var clrType = domainEvent.GetType();

        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            Type = clrType.AssemblyQualifiedName!,
            Payload = JsonSerializer.Serialize(domainEvent, clrType),
            OccurredOnUtc = domainEvent.OccurredOn,
            RetryCount = 0,
        };
    }

    public void MarkProcessed(DateTimeOffset processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error.Length > 2000 ? error[..2000] : error;
        RetryCount++;
    }
}
