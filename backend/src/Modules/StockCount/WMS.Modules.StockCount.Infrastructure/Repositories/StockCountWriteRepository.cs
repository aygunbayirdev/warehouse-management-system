using Microsoft.EntityFrameworkCore;
using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Infrastructure.Persistence;

namespace WMS.Modules.StockCount.Infrastructure.Repositories;

internal sealed class StockCountWriteRepository(StockCountDbContext dbContext) : IStockCountWriteRepository
{
    public Task<StockCountAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.StockCounts
            .Include(stockCount => stockCount.Lines)
            .FirstOrDefaultAsync(stockCount => stockCount.Id == id, cancellationToken);

    public void Add(StockCountAggregate stockCount) => dbContext.StockCounts.Add(stockCount);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
