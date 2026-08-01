using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.Abstractions;

public interface IStockCountReadRepository
{
    Task<StockCountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<StockCountDto>> GetListAsync(
        Guid? warehouseId,
        StockCountStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockCountVarianceReportRowDto>> GetVarianceReportAsync(
        Guid? warehouseId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken);
}
