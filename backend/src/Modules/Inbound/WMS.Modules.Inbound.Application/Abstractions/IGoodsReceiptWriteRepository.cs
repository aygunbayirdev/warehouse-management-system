using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Application.Abstractions;

public interface IGoodsReceiptWriteRepository
{
    Task<GoodsReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(GoodsReceipt goodsReceipt);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
