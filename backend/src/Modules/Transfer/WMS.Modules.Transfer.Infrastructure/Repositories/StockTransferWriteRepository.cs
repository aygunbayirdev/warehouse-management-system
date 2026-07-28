using Microsoft.EntityFrameworkCore;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Domain;
using WMS.Modules.Transfer.Infrastructure.Persistence;

namespace WMS.Modules.Transfer.Infrastructure.Repositories;

internal sealed class StockTransferWriteRepository(TransferDbContext dbContext) : IStockTransferWriteRepository
{
    public Task<StockTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.StockTransfers
            .Include(stockTransfer => stockTransfer.Lines)
            .FirstOrDefaultAsync(stockTransfer => stockTransfer.Id == id, cancellationToken);

    public void Add(StockTransfer stockTransfer) => dbContext.StockTransfers.Add(stockTransfer);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
