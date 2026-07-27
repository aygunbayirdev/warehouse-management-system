using Microsoft.EntityFrameworkCore;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Domain;
using WMS.Modules.Inbound.Infrastructure.Persistence;

namespace WMS.Modules.Inbound.Infrastructure.Repositories;

internal sealed class GoodsReceiptWriteRepository(InboundDbContext dbContext) : IGoodsReceiptWriteRepository
{
    public Task<GoodsReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.GoodsReceipts
            .Include(goodsReceipt => goodsReceipt.Lines)
            .FirstOrDefaultAsync(goodsReceipt => goodsReceipt.Id == id, cancellationToken);

    public void Add(GoodsReceipt goodsReceipt) => dbContext.GoodsReceipts.Add(goodsReceipt);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
