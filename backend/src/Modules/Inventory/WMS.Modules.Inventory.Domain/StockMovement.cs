using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Domain;

/// <summary>Append-only audit ledger entry. Every stock quantity change must produce exactly one of these.</summary>
public sealed class StockMovement : BaseEntity
{
    private StockMovement()
    {
    }

    private StockMovement(
        Guid id,
        Guid warehouseId,
        Guid productId,
        StockMovementType type,
        decimal quantity,
        string reason,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        WarehouseId = warehouseId;
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        Reason = reason;
        OccurredAtUtc = occurredAtUtc;
    }

    public Guid WarehouseId { get; private set; }

    public Guid ProductId { get; private set; }

    public StockMovementType Type { get; private set; }

    public decimal Quantity { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static StockMovement Create(Guid warehouseId, Guid productId, StockMovementType type, decimal quantity, string reason)
    {
        Guard.AgainstEmpty(warehouseId, nameof(warehouseId));
        Guard.AgainstEmpty(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));

        return new StockMovement(Guid.CreateVersion7(), warehouseId, productId, type, quantity, reason.Trim(), DateTimeOffset.UtcNow);
    }
}
