using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Domain;

public sealed class StockTransferLine : BaseEntity
{
    private StockTransferLine()
    {
    }

    private StockTransferLine(Guid id, Guid stockTransferId, Guid productId, decimal quantity)
        : base(id)
    {
        StockTransferId = stockTransferId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid StockTransferId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    internal static StockTransferLine Create(Guid stockTransferId, Guid productId, decimal quantity)
    {
        Guard.AgainstEmpty(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        return new StockTransferLine(Guid.CreateVersion7(), stockTransferId, productId, quantity);
    }
}
