using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Domain;

public sealed record GoodsReceiptApprovedLine(Guid ProductId, decimal Quantity);

/// <summary>
/// Raised when a GoodsReceipt is approved. The Inbound module's own handler reacts to this and
/// sends IncreaseStockCommand (Inventory module's public contract) for each line — Inventory never
/// references Inbound, keeping the module dependency direction one-way.
/// </summary>
public sealed record GoodsReceiptApprovedDomainEvent(
    Guid GoodsReceiptId,
    Guid WarehouseId,
    IReadOnlyCollection<GoodsReceiptApprovedLine> Lines,
    DateTimeOffset OccurredOn) : IDomainEvent;
