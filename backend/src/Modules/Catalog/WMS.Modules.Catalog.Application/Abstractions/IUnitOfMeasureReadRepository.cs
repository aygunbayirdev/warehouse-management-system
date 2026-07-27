using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Abstractions;

public interface IUnitOfMeasureReadRepository
{
    Task<UnitOfMeasureDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<UnitOfMeasureDto>> GetAllAsync(CancellationToken cancellationToken);
}
