using Microsoft.EntityFrameworkCore;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Domain;
using WMS.Modules.Outbound.Infrastructure.Persistence;

namespace WMS.Modules.Outbound.Infrastructure.Repositories;

internal sealed class GoodsIssueWriteRepository(OutboundDbContext dbContext) : IGoodsIssueWriteRepository
{
    public Task<GoodsIssue?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.GoodsIssues
            .Include(goodsIssue => goodsIssue.Lines)
            .FirstOrDefaultAsync(goodsIssue => goodsIssue.Id == id, cancellationToken);

    public void Add(GoodsIssue goodsIssue) => dbContext.GoodsIssues.Add(goodsIssue);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
