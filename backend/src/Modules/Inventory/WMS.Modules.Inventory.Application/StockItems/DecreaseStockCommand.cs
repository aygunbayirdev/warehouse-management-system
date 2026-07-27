using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Inventory.Application.StockItems;

/// <summary>
/// Public cross-module entry point for decreasing stock (e.g. a Goods Issue approval). Other
/// modules send this via MediatR from their own domain-event handlers instead of touching
/// StockItem/StockMovement directly, keeping module boundaries intact.
/// </summary>
public sealed record DecreaseStockCommand(Guid WarehouseId, Guid ProductId, decimal Quantity, string Reason) : ICommand;
