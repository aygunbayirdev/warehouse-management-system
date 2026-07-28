using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Domain;

public sealed class StockCountAdjustment : BaseEntity
{
    private StockCountAdjustment()
    {
    }

    private StockCountAdjustment(
        Guid id,
        Guid stockCountId,
        Guid stockCountLineId,
        Guid warehouseId,
        Guid productId,
        decimal differenceQuantity)
        : base(id)
    {
        StockCountId = stockCountId;
        StockCountLineId = stockCountLineId;
        WarehouseId = warehouseId;
        ProductId = productId;
        DifferenceQuantity = differenceQuantity;
        Status = StockCountAdjustmentStatus.Pending;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid StockCountId { get; private set; }

    public Guid StockCountLineId { get; private set; }

    public Guid WarehouseId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal DifferenceQuantity { get; private set; }

    public StockCountAdjustmentStatus Status { get; private set; }

    public Guid? ApprovedByUserId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? DecidedAtUtc { get; private set; }

    public static StockCountAdjustment Create(
        Guid stockCountId,
        Guid stockCountLineId,
        Guid warehouseId,
        Guid productId,
        decimal differenceQuantity)
    {
        Guard.AgainstEmpty(stockCountId, nameof(stockCountId));
        Guard.AgainstEmpty(stockCountLineId, nameof(stockCountLineId));
        Guard.AgainstEmpty(warehouseId, nameof(warehouseId));
        Guard.AgainstEmpty(productId, nameof(productId));

        if (differenceQuantity == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(differenceQuantity), differenceQuantity, "Value cannot be zero.");
        }

        return new StockCountAdjustment(Guid.CreateVersion7(), stockCountId, stockCountLineId, warehouseId, productId, differenceQuantity);
    }

    public Result Approve(Guid approvedByUserId)
    {
        if (Status != StockCountAdjustmentStatus.Pending)
        {
            return Result.Failure(
                Error.Conflict("StockCountAdjustment.NotPending", "Only a pending stock count adjustment can be approved."));
        }

        Status = StockCountAdjustmentStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        DecidedAtUtc = DateTimeOffset.UtcNow;

        AddDomainEvent(new StockCountAdjustmentApprovedDomainEvent(Id, WarehouseId, ProductId, DifferenceQuantity, DecidedAtUtc.Value));

        return Result.Success();
    }

    public Result Reject(Guid approvedByUserId)
    {
        if (Status != StockCountAdjustmentStatus.Pending)
        {
            return Result.Failure(
                Error.Conflict("StockCountAdjustment.NotPending", "Only a pending stock count adjustment can be rejected."));
        }

        Status = StockCountAdjustmentStatus.Rejected;
        ApprovedByUserId = approvedByUserId;
        DecidedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
