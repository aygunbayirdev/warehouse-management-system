using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Domain;

public sealed class StockTransfer : BaseEntity
{
    private readonly List<StockTransferLine> _lines = [];

    private StockTransfer()
    {
    }

    private StockTransfer(Guid id, Guid sourceWarehouseId, Guid destinationWarehouseId, Guid createdByUserId)
        : base(id)
    {
        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        CreatedByUserId = createdByUserId;
        Status = StockTransferStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid SourceWarehouseId { get; private set; }

    public Guid DestinationWarehouseId { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public StockTransferStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ShippedAtUtc { get; private set; }

    public DateTimeOffset? ReceivedAtUtc { get; private set; }

    public IReadOnlyCollection<StockTransferLine> Lines => _lines.AsReadOnly();

    public static Result<StockTransfer> Create(Guid sourceWarehouseId, Guid destinationWarehouseId, Guid createdByUserId)
    {
        Guard.AgainstEmpty(sourceWarehouseId, nameof(sourceWarehouseId));
        Guard.AgainstEmpty(destinationWarehouseId, nameof(destinationWarehouseId));
        Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));

        if (sourceWarehouseId == destinationWarehouseId)
        {
            return Result.Failure<StockTransfer>(
                Error.Validation("StockTransfer.SameWarehouse", "Source and destination warehouses must be different."));
        }

        return new StockTransfer(Guid.CreateVersion7(), sourceWarehouseId, destinationWarehouseId, createdByUserId);
    }

    public Result AddLine(Guid productId, decimal quantity)
    {
        if (Status != StockTransferStatus.Draft)
        {
            return Result.Failure(
                Error.Conflict("StockTransfer.NotDraft", "Lines can only be added while the stock transfer is in Draft status."));
        }

        _lines.Add(StockTransferLine.Create(Id, productId, quantity));

        return Result.Success();
    }

    public Result Ship()
    {
        if (Status != StockTransferStatus.Draft)
        {
            return Result.Failure(Error.Conflict("StockTransfer.NotDraft", "Only a draft stock transfer can be shipped."));
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(
                Error.Validation("StockTransfer.NoLines", "A stock transfer must have at least one line before it can be shipped."));
        }

        Status = StockTransferStatus.Shipped;
        ShippedAtUtc = DateTimeOffset.UtcNow;

        var lines = _lines.Select(line => new StockTransferShippedLine(line.ProductId, line.Quantity)).ToList();
        AddDomainEvent(new StockTransferShippedDomainEvent(Id, SourceWarehouseId, lines, ShippedAtUtc.Value));

        return Result.Success();
    }

    public Result Receive()
    {
        if (Status != StockTransferStatus.Shipped)
        {
            return Result.Failure(Error.Conflict("StockTransfer.NotShipped", "Only a shipped stock transfer can be received."));
        }

        Status = StockTransferStatus.Received;
        ReceivedAtUtc = DateTimeOffset.UtcNow;

        var lines = _lines.Select(line => new StockTransferReceivedLine(line.ProductId, line.Quantity)).ToList();
        AddDomainEvent(new StockTransferReceivedDomainEvent(Id, DestinationWarehouseId, lines, ReceivedAtUtc.Value));

        return Result.Success();
    }
}
