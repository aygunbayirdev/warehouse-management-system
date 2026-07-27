using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Abstractions;

public interface IProductReadRepository
{
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProductDto>> GetListAsync(Guid? categoryId, string? search, CancellationToken cancellationToken);
}
