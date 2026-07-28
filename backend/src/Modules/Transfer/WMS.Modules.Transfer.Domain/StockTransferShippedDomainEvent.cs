using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Domain;

public sealed record StockTransferShippedLine(Guid ProductId, decimal Quantity);

/// <summary>
/// Raised when a StockTransfer is shipped from the source warehouse. The Transfer module's own
/// handler reacts to this and sends DecreaseStockCommand (Inventory module's public contract) for
/// each line — Inventory never references Transfer, keeping the module dependency direction one-way.
/// </summary>
public sealed record StockTransferShippedDomainEvent(
    Guid StockTransferId,
    Guid SourceWarehouseId,
    IReadOnlyCollection<StockTransferShippedLine> Lines,
    DateTimeOffset OccurredOn) : IDomainEvent;
