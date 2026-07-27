using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Domain;

public sealed class GoodsReceipt : BaseEntity
{
    private readonly List<GoodsReceiptLine> _lines = [];

    private GoodsReceipt()
    {
    }

    private GoodsReceipt(Guid id, Guid warehouseId, Guid createdByUserId)
        : base(id)
    {
        WarehouseId = warehouseId;
        CreatedByUserId = createdByUserId;
        Status = GoodsReceiptStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid WarehouseId { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public GoodsReceiptStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public IReadOnlyCollection<GoodsReceiptLine> Lines => _lines.AsReadOnly();

    public static GoodsReceipt Create(Guid warehouseId, Guid createdByUserId)
    {
        Guard.AgainstEmpty(warehouseId, nameof(warehouseId));
        Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));

        return new GoodsReceipt(Guid.CreateVersion7(), warehouseId, createdByUserId);
    }

    public Result AddLine(Guid productId, decimal quantity)
    {
        if (Status != GoodsReceiptStatus.Draft)
        {
            return Result.Failure(
                Error.Conflict("GoodsReceipt.NotDraft", "Lines can only be added while the goods receipt is in Draft status."));
        }

        _lines.Add(GoodsReceiptLine.Create(Id, productId, quantity));

        return Result.Success();
    }

    public Result Approve()
    {
        if (Status != GoodsReceiptStatus.Draft)
        {
            return Result.Failure(Error.Conflict("GoodsReceipt.NotDraft", "Only a draft goods receipt can be approved."));
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(
                Error.Validation("GoodsReceipt.NoLines", "A goods receipt must have at least one line before it can be approved."));
        }

        Status = GoodsReceiptStatus.Approved;
        ApprovedAtUtc = DateTimeOffset.UtcNow;

        var lines = _lines.Select(line => new GoodsReceiptApprovedLine(line.ProductId, line.Quantity)).ToList();
        AddDomainEvent(new GoodsReceiptApprovedDomainEvent(Id, WarehouseId, lines, ApprovedAtUtc.Value));

        return Result.Success();
    }
}
