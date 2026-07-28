using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Domain;

public sealed class StockCount : BaseEntity
{
    private readonly List<StockCountLine> _lines = [];

    private StockCount()
    {
    }

    private StockCount(Guid id, Guid warehouseId, Guid createdByUserId)
        : base(id)
    {
        WarehouseId = warehouseId;
        CreatedByUserId = createdByUserId;
        Status = StockCountStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid WarehouseId { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public StockCountStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public IReadOnlyCollection<StockCountLine> Lines => _lines.AsReadOnly();

    public static StockCount Create(Guid warehouseId, Guid createdByUserId)
    {
        Guard.AgainstEmpty(warehouseId, nameof(warehouseId));
        Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));

        return new StockCount(Guid.CreateVersion7(), warehouseId, createdByUserId);
    }

    public Result Start()
    {
        if (Status != StockCountStatus.Draft)
        {
            return Result.Failure(Error.Conflict("StockCount.NotDraft", "Only a draft stock count can be started."));
        }

        Status = StockCountStatus.InProgress;

        return Result.Success();
    }

    public Result AddLine(Guid productId, decimal systemQuantity, decimal countedQuantity)
    {
        if (Status != StockCountStatus.InProgress)
        {
            return Result.Failure(
                Error.Conflict("StockCount.NotInProgress", "Lines can only be entered while the stock count is in progress."));
        }

        if (_lines.Any(line => line.ProductId == productId))
        {
            return Result.Failure(
                Error.Conflict("StockCount.DuplicateLine", "This product has already been counted in this stock count."));
        }

        _lines.Add(StockCountLine.Create(Id, productId, systemQuantity, countedQuantity));

        return Result.Success();
    }

    public Result Complete()
    {
        if (Status != StockCountStatus.InProgress)
        {
            return Result.Failure(Error.Conflict("StockCount.NotInProgress", "Only an in-progress stock count can be completed."));
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(
                Error.Validation("StockCount.NoLines", "A stock count must have at least one line before it can be completed."));
        }

        Status = StockCountStatus.Completed;
        ClosedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
