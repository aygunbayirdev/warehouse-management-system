using Microsoft.EntityFrameworkCore;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Domain;
using WMS.Modules.Inventory.Infrastructure.Persistence;

namespace WMS.Modules.Inventory.Infrastructure.Repositories;

internal sealed class WarehouseWriteRepository(InventoryDbContext dbContext) : IWarehouseWriteRepository
{
    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Warehouses.FirstOrDefaultAsync(warehouse => warehouse.Id == id, cancellationToken);

    public Task<Warehouse?> GetByCodeAsync(string code, CancellationToken cancellationToken) =>
        dbContext.Warehouses.FirstOrDefaultAsync(warehouse => warehouse.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public void Add(Warehouse warehouse) => dbContext.Warehouses.Add(warehouse);

    public void Remove(Warehouse warehouse) => dbContext.Warehouses.Remove(warehouse);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
