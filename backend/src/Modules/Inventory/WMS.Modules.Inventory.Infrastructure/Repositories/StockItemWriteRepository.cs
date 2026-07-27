using Microsoft.EntityFrameworkCore;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Domain;
using WMS.Modules.Inventory.Infrastructure.Persistence;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Infrastructure.Repositories;

internal sealed class StockItemWriteRepository(InventoryDbContext dbContext) : IStockItemWriteRepository
{
    public async Task<StockItem> GetOrCreateAsync(Guid warehouseId, Guid productId, CancellationToken cancellationToken)
    {
        var stockItem = await dbContext.StockItems.FirstOrDefaultAsync(
            item => item.WarehouseId == warehouseId && item.ProductId == productId,
            cancellationToken);

        if (stockItem is not null)
        {
            return stockItem;
        }

        stockItem = StockItem.Create(warehouseId, productId);
        dbContext.StockItems.Add(stockItem);

        return stockItem;
    }

    public Task<bool> ExistsWithWarehouseIdAsync(Guid warehouseId, CancellationToken cancellationToken) =>
        dbContext.StockItems.AnyAsync(stockItem => stockItem.WarehouseId == warehouseId, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException("The stock item was modified by another operation.", ex);
        }
    }
}
