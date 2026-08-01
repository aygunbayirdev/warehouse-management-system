namespace WMS.Modules.Inventory.Domain;

/// <summary>
/// Idempotency ledger for cross-module stock mutations delivered via the outbox relay (at-least-once
/// delivery, see WMS.BuildingBlocks.Infrastructure.Outbox). Keyed by (SourceEventId, LineNumber):
/// SourceEventId is the producing module's outbox message id, LineNumber disambiguates the N stock
/// commands that can fan out from one multi-line event (e.g. a GoodsReceipt with several lines).
/// Deliberately not a BaseEntity — it never raises domain events and needs a composite key, not the
/// single-Guid Id BaseEntity provides.
/// </summary>
public sealed class ProcessedDomainEvent
{
    private ProcessedDomainEvent()
    {
    }

    public ProcessedDomainEvent(Guid sourceEventId, int lineNumber, DateTimeOffset processedAtUtc)
    {
        SourceEventId = sourceEventId;
        LineNumber = lineNumber;
        ProcessedAtUtc = processedAtUtc;
    }

    public Guid SourceEventId { get; private set; }

    public int LineNumber { get; private set; }

    public DateTimeOffset ProcessedAtUtc { get; private set; }
}
