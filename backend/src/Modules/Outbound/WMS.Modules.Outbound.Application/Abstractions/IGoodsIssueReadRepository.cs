using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Outbound.Application.Dtos;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Application.Abstractions;

public interface IGoodsIssueReadRepository
{
    Task<GoodsIssueDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<GoodsIssueDto>> GetListAsync(
        Guid? warehouseId,
        GoodsIssueStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
