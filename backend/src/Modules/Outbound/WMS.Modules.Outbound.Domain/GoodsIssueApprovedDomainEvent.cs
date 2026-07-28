using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Domain;

public sealed record GoodsIssueApprovedLine(Guid ProductId, decimal Quantity);

/// <summary>
/// Raised when a GoodsIssue is approved. The Outbound module's own handler reacts to this and
/// sends DecreaseStockCommand (Inventory module's public contract) for each line — Inventory never
/// references Outbound, keeping the module dependency direction one-way.
/// </summary>
public sealed record GoodsIssueApprovedDomainEvent(
    Guid GoodsIssueId,
    Guid WarehouseId,
    IReadOnlyCollection<GoodsIssueApprovedLine> Lines,
    DateTimeOffset OccurredOn) : IDomainEvent;
