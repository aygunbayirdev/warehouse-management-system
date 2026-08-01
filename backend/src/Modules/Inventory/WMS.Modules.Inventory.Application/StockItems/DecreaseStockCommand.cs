using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Inventory.Application.StockItems;

/// <summary>
/// Public cross-module entry point for decreasing stock (e.g. a Goods Issue approval). Other
/// modules send this via MediatR from their own domain-event handlers instead of touching
/// StockItem/StockMovement directly, keeping module boundaries intact.
/// <paramref name="SourceEventId"/>/<paramref name="LineNumber"/> identify the outbox message + line
/// this command was raised from — the handler uses them as an idempotency key, since the outbox relay
/// that triggers domain-event handlers guarantees at-least-once, not exactly-once, delivery.
/// </summary>
public sealed record DecreaseStockCommand(
    Guid SourceEventId,
    int LineNumber,
    Guid WarehouseId,
    Guid ProductId,
    decimal Quantity,
    string Reason) : ICommand;
