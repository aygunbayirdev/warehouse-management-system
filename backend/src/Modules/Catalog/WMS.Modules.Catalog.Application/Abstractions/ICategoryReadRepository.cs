using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Abstractions;

public interface ICategoryReadRepository
{
    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
}
