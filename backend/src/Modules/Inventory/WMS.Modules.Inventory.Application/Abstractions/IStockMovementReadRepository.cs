using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IStockMovementReadRepository
{
    Task<PagedResult<StockMovementDto>> GetListAsync(
        Guid? warehouseId,
        Guid? productId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
