using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed record DeleteWarehouseCommand(Guid Id) : ICommand;
