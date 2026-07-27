using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed record CreateWarehouseCommand(string Code, string Name, string? Address) : ICommand<Guid>;
