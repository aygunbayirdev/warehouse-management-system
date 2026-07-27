using Microsoft.EntityFrameworkCore;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Domain;
using WMS.Modules.Catalog.Infrastructure.Persistence;

namespace WMS.Modules.Catalog.Infrastructure.Repositories;

internal sealed class UnitOfMeasureWriteRepository(CatalogDbContext dbContext) : IUnitOfMeasureWriteRepository
{
    public Task<UnitOfMeasure?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.UnitsOfMeasure.FirstOrDefaultAsync(unitOfMeasure => unitOfMeasure.Id == id, cancellationToken);

    public Task<UnitOfMeasure?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        dbContext.UnitsOfMeasure.FirstOrDefaultAsync(unitOfMeasure => unitOfMeasure.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public void Add(UnitOfMeasure unitOfMeasure) => dbContext.UnitsOfMeasure.Add(unitOfMeasure);

    public void Remove(UnitOfMeasure unitOfMeasure) => dbContext.UnitsOfMeasure.Remove(unitOfMeasure);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
