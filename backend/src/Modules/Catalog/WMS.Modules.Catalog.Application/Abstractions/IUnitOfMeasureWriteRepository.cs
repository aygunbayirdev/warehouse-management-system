using WMS.Modules.Catalog.Domain;

namespace WMS.Modules.Catalog.Application.Abstractions;

public interface IUnitOfMeasureWriteRepository
{
    Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    void Add(UnitOfMeasure unitOfMeasure);

    void Remove(UnitOfMeasure unitOfMeasure);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
