using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Domain;

/// <summary>
/// Raised when a StockCountAdjustment is approved. The StockCount module's own handler reacts to
/// this and sends IncreaseStockCommand or DecreaseStockCommand (Inventory module's public contract,
/// chosen by the sign of DifferenceQuantity) — Inventory never references StockCount, keeping the
/// module dependency direction one-way.
/// </summary>
public sealed record StockCountAdjustmentApprovedDomainEvent(
    Guid StockCountAdjustmentId,
    Guid WarehouseId,
    Guid ProductId,
    decimal DifferenceQuantity,
    DateTimeOffset OccurredOn) : IDomainEvent;
