using Microsoft.EntityFrameworkCore;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Domain;
using WMS.Modules.Catalog.Infrastructure.Persistence;

namespace WMS.Modules.Catalog.Infrastructure.Repositories;

internal sealed class ProductWriteRepository(CatalogDbContext dbContext) : IProductWriteRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(product => product.Sku == sku.Trim().ToUpperInvariant(), cancellationToken);

    public Task<bool> ExistsWithUnitOfMeasureIdAsync(Guid unitOfMeasureId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.UnitOfMeasureId == unitOfMeasureId, cancellationToken);

    public Task<bool> ExistsWithCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.CategoryId == categoryId, cancellationToken);

    public void Add(Product product) => dbContext.Products.Add(product);

    public void Remove(Product product) => dbContext.Products.Remove(product);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
