using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Domain;

public sealed class GoodsIssueLine : BaseEntity
{
    private GoodsIssueLine()
    {
    }

    private GoodsIssueLine(Guid id, Guid goodsIssueId, Guid productId, decimal quantity)
        : base(id)
    {
        GoodsIssueId = goodsIssueId;
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid GoodsIssueId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    internal static GoodsIssueLine Create(Guid goodsIssueId, Guid productId, decimal quantity)
    {
        Guard.AgainstEmpty(productId, nameof(productId));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));

        return new GoodsIssueLine(Guid.CreateVersion7(), goodsIssueId, productId, quantity);
    }
}
