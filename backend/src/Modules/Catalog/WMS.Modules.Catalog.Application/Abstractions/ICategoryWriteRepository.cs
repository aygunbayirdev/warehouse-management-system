using WMS.Modules.Catalog.Domain;

namespace WMS.Modules.Catalog.Application.Abstractions;

public interface ICategoryWriteRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken);

    void Add(Category category);

    void Remove(Category category);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
