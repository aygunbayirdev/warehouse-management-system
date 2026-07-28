using WMS.Modules.Outbound.Application.Dtos;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Application.Abstractions;

public interface IGoodsIssueReadRepository
{
    Task<GoodsIssueDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<GoodsIssueDto>> GetListAsync(
        Guid? warehouseId,
        GoodsIssueStatus? status,
        CancellationToken cancellationToken);
}
