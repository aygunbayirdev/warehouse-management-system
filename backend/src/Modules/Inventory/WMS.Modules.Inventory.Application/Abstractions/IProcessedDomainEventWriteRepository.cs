using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IProcessedDomainEventWriteRepository
{
    /// <summary>Whether (sourceEventId, lineNumber) has already been applied — an at-least-once redelivery.</summary>
    Task<bool> ExistsAsync(Guid sourceEventId, int lineNumber, CancellationToken cancellationToken);

    /// <summary>Stages a ledger row; committed in the same SaveChangesAsync as the stock mutation it guards.</summary>
    void Add(ProcessedDomainEvent processedDomainEvent);
}
