using WMS.Modules.Catalog.Domain;

namespace WMS.Modules.Catalog.Application.Abstractions;

public interface IProductWriteRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken);

    Task<bool> ExistsWithUnitOfMeasureIdAsync(Guid unitOfMeasureId, CancellationToken cancellationToken);

    Task<bool> ExistsWithCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken);

    void Add(Product product);

    void Remove(Product product);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
