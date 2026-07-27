using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Domain;

public sealed class GoodsReceiptLine : BaseEntity
{
    private GoodsReceiptLine()
    {
    }

    private GoodsReceiptLine(Guid id, Guid goodsReceiptId, Guid productId, decimal quantity)
        : base(id)
    {
        GoodsReceiptId = goodsReceiptId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid GoodsReceiptId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    internal static GoodsReceiptLine Create(Guid goodsReceiptId, Guid productId, decimal quantity)
    {
        Guard.AgainstEmpty(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        return new GoodsReceiptLine(Guid.CreateVersion7(), goodsReceiptId, productId, quantity);
    }
}
