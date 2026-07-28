using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;

namespace WMS.Modules.StockCount.Application.Abstractions;

public interface IStockCountWriteRepository
{
    Task<StockCountAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(StockCountAggregate stockCount);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
