using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.Abstractions;

public interface IStockCountAdjustmentReadRepository
{
    Task<StockCountAdjustmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<StockCountAdjustmentDto>> GetListAsync(
        Guid? warehouseId,
        StockCountAdjustmentStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
