using Microsoft.EntityFrameworkCore;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Domain;
using WMS.Modules.Inventory.Infrastructure.Persistence;

namespace WMS.Modules.Inventory.Infrastructure.Repositories;

internal sealed class ProcessedDomainEventWriteRepository(InventoryDbContext dbContext) : IProcessedDomainEventWriteRepository
{
    public Task<bool> ExistsAsync(Guid sourceEventId, int lineNumber, CancellationToken cancellationToken) =>
        dbContext.ProcessedDomainEvents.AnyAsync(
            p => p.SourceEventId == sourceEventId && p.LineNumber == lineNumber,
            cancellationToken);

    public void Add(ProcessedDomainEvent processedDomainEvent) =>
        dbContext.ProcessedDomainEvents.Add(processedDomainEvent);
}
