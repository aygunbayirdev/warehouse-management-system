using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class IncreaseStockCommandHandler(
    IStockItemWriteRepository stockItemWriteRepository,
    IStockMovementWriteRepository stockMovementWriteRepository,
    IProcessedDomainEventWriteRepository processedDomainEventWriteRepository)
    : ICommandHandler<IncreaseStockCommand>
{
    public async Task<Result> Handle(IncreaseStockCommand request, CancellationToken cancellationToken)
    {
        // The outbox relay guarantees at-least-once delivery; a redelivered command must not double-apply.
        if (await processedDomainEventWriteRepository.ExistsAsync(request.SourceEventId, request.LineNumber, cancellationToken))
        {
            return Result.Success();
        }

        var stockItem = await stockItemWriteRepository.GetOrCreateAsync(request.WarehouseId, request.ProductId, cancellationToken);

        stockItem.Increase(request.Quantity);

        var movement = StockMovement.Create(
            request.WarehouseId,
            request.ProductId,
            StockMovementType.Increase,
            request.Quantity,
            request.Reason);

        stockMovementWriteRepository.Add(movement);

        processedDomainEventWriteRepository.Add(
            new ProcessedDomainEvent(request.SourceEventId, request.LineNumber, DateTimeOffset.UtcNow));

        try
        {
            await stockItemWriteRepository.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure(
                Error.Conflict("StockItem.ConcurrencyConflict", "The stock item was modified concurrently. Please retry."));
        }

        return Result.Success();
    }
}
