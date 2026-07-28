using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Domain;

public sealed class StockCountLine : BaseEntity
{
    private StockCountLine()
    {
    }

    private StockCountLine(Guid id, Guid stockCountId, Guid productId, decimal systemQuantity, decimal countedQuantity)
        : base(id)
    {
        StockCountId = stockCountId;
        ProductId = productId;
        SystemQuantity = systemQuantity;
        CountedQuantity = countedQuantity;
        Difference = countedQuantity - systemQuantity;
    }

    public Guid StockCountId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal SystemQuantity { get; private set; }

    public decimal CountedQuantity { get; private set; }

    public decimal Difference { get; private set; }

    internal static StockCountLine Create(Guid stockCountId, Guid productId, decimal systemQuantity, decimal countedQuantity)
    {
        Guard.AgainstEmpty(productId, nameof(productId));
        Guard.AgainstNegative(systemQuantity, nameof(systemQuantity));
        Guard.AgainstNegative(countedQuantity, nameof(countedQuantity));

        return new StockCountLine(Guid.CreateVersion7(), stockCountId, productId, systemQuantity, countedQuantity);
    }
}
