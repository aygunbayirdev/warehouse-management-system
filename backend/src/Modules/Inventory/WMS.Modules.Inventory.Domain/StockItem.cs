using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Domain;

/// <summary>
/// The current quantity of one product in one warehouse. Mutated concurrently by Inbound, Outbound,
/// Transfer, and StockCount adjustment flows, so callers must handle optimistic-concurrency conflicts
/// (Postgres xmin, configured in Infrastructure) when saving.
/// </summary>
public sealed class StockItem : BaseEntity
{
    private StockItem()
    {
    }

    private StockItem(Guid id, Guid warehouseId, Guid productId, decimal quantity)
        : base(id)
    {
        WarehouseId = warehouseId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid WarehouseId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public static StockItem Create(Guid warehouseId, Guid productId)
    {
        Guard.AgainstEmpty(warehouseId, nameof(warehouseId));
        Guard.AgainstEmpty(productId, nameof(productId));

        return new StockItem(Guid.CreateVersion7(), warehouseId, productId, 0m);
    }

    public void Increase(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        Quantity += amount;
    }

    public Result Decrease(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        if (Quantity < amount)
        {
            return Result.Failure(
                Error.Conflict("StockItem.InsufficientStock", "There is not enough stock to complete this operation."));
        }

        Quantity -= amount;

        return Result.Success();
    }
}
