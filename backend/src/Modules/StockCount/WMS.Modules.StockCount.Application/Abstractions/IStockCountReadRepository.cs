using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.Abstractions;

public interface IStockCountReadRepository
{
    Task<StockCountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockCountDto>> GetListAsync(
        Guid? warehouseId,
        StockCountStatus? status,
        CancellationToken cancellationToken);
}
