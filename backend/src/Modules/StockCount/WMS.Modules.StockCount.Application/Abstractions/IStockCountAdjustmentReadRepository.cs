using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.Abstractions;

public interface IStockCountAdjustmentReadRepository
{
    Task<StockCountAdjustmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockCountAdjustmentDto>> GetListAsync(
        Guid? warehouseId,
        StockCountAdjustmentStatus? status,
        CancellationToken cancellationToken);
}
