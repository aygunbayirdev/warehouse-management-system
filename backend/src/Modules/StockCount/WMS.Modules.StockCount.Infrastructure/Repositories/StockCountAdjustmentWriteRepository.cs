using Microsoft.EntityFrameworkCore;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Domain;
using WMS.Modules.StockCount.Infrastructure.Persistence;

namespace WMS.Modules.StockCount.Infrastructure.Repositories;

internal sealed class StockCountAdjustmentWriteRepository(StockCountDbContext dbContext) : IStockCountAdjustmentWriteRepository
{
    public Task<StockCountAdjustment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.StockCountAdjustments.FirstOrDefaultAsync(adjustment => adjustment.Id == id, cancellationToken);

    public void Add(StockCountAdjustment stockCountAdjustment) => dbContext.StockCountAdjustments.Add(stockCountAdjustment);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
