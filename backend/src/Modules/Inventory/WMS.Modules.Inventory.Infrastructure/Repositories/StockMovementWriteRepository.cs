using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Domain;
using WMS.Modules.Inventory.Infrastructure.Persistence;

namespace WMS.Modules.Inventory.Infrastructure.Repositories;

internal sealed class StockMovementWriteRepository(InventoryDbContext dbContext) : IStockMovementWriteRepository
{
    public void Add(StockMovement stockMovement) => dbContext.StockMovements.Add(stockMovement);
}
