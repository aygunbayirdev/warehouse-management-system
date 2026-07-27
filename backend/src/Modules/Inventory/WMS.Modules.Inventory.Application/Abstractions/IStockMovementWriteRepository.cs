using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IStockMovementWriteRepository
{
    void Add(StockMovement stockMovement);
}
