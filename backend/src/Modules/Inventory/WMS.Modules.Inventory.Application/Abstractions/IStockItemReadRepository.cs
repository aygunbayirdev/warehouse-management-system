using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IStockItemReadRepository
{
    Task<IReadOnlyCollection<StockItemDto>> GetListAsync(Guid? warehouseId, Guid? productId, CancellationToken cancellationToken);
}
