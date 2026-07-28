using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IStockMovementReadRepository
{
    Task<IReadOnlyCollection<StockMovementDto>> GetListAsync(
        Guid? warehouseId,
        Guid? productId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken);
}
