using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.Abstractions;

public interface IStockCountAdjustmentWriteRepository
{
    Task<StockCountAdjustment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(StockCountAdjustment stockCountAdjustment);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
