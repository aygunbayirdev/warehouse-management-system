using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Application.Abstractions;

public interface IStockTransferWriteRepository
{
    Task<StockTransfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(StockTransfer stockTransfer);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
