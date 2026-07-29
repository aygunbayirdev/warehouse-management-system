using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Abstractions;

public interface IProductReadRepository
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ProductDto>> GetListAsync(
        Guid? categoryId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
