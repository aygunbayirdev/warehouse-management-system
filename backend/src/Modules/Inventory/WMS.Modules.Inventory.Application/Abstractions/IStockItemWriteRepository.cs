using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IStockItemWriteRepository
{
    /// <summary>Loads the tracked StockItem for (warehouseId, productId), creating one with Quantity=0 if none exists yet.</summary>
    Task<StockItem> GetOrCreateAsync(Guid warehouseId, Guid productId, CancellationToken cancellationToken);

    Task<bool> ExistsWithWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken);

    /// <summary>
    /// Persists all pending changes on this module's DbContext (StockItem + any staged StockMovement).
    /// Throws <see cref="WMS.SharedKernel.ConcurrencyConflictException"/> on an xmin mismatch.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
