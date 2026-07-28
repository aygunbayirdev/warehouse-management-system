using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Application.Abstractions;

public interface IGoodsIssueWriteRepository
{
    Task<GoodsIssue?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(GoodsIssue goodsIssue);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
