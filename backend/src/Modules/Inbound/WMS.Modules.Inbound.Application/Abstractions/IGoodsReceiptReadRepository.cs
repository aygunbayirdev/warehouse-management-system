using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Inbound.Application.Dtos;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Application.Abstractions;

public interface IGoodsReceiptReadRepository
{
    Task<GoodsReceiptDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<GoodsReceiptDto>> GetListAsync(
        Guid? warehouseId,
        GoodsReceiptStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
