using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Transfer.Application.Dtos;
using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Application.Abstractions;

public interface IStockTransferReadRepository
{
    Task<StockTransferDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<StockTransferDto>> GetListAsync(
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        StockTransferStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
