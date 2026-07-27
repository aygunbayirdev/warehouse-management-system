using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IWarehouseWriteRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken);

    void Add(Warehouse warehouse);

    void Remove(Warehouse warehouse);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
