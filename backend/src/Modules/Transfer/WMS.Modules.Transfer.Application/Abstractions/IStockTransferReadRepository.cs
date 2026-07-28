using WMS.Modules.Transfer.Application.Dtos;
using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Application.Abstractions;

public interface IStockTransferReadRepository
{
    Task<StockTransferDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockTransferDto>> GetListAsync(
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        StockTransferStatus? status,
        CancellationToken cancellationToken);
}
