using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed record GetWarehousesQuery : IQuery<IReadOnlyCollection<WarehouseDto>>;
