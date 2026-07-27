using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.Abstractions;

public interface IWarehouseReadRepository
{
    Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken);
}
