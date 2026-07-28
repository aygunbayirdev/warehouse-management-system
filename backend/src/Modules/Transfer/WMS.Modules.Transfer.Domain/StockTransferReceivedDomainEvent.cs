using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Domain;

public sealed record StockTransferReceivedLine(Guid ProductId, decimal Quantity);

/// <summary>
/// Raised when a StockTransfer is received at the destination warehouse. The Transfer module's own
/// handler reacts to this and sends IncreaseStockCommand (Inventory module's public contract) for
/// each line — Inventory never references Transfer, keeping the module dependency direction one-way.
/// </summary>
public sealed record StockTransferReceivedDomainEvent(
    Guid StockTransferId,
    Guid DestinationWarehouseId,
    IReadOnlyCollection<StockTransferReceivedLine> Lines,
    DateTimeOffset OccurredOn) : IDomainEvent;
