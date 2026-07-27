using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed record UpdateWarehouseCommand(Guid Id, string Name, string? Address) : ICommand;
