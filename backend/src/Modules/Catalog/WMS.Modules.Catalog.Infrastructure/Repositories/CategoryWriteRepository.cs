using Microsoft.EntityFrameworkCore;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Domain;
using WMS.Modules.Catalog.Infrastructure.Persistence;

namespace WMS.Modules.Catalog.Infrastructure.Repositories;

internal sealed class CategoryWriteRepository(CatalogDbContext dbContext) : ICategoryWriteRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Categories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
        dbContext.Categories.FirstOrDefaultAsync(category => category.Name == name.Trim(), cancellationToken);

    public void Add(Category category) => dbContext.Categories.Add(category);

    public void Remove(Category category) => dbContext.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
