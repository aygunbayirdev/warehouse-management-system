using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Domain;

public sealed class GoodsIssue : BaseEntity
{
    private readonly List<GoodsIssueLine> _lines = [];

    private GoodsIssue()
    {
    }

    private GoodsIssue(Guid id, Guid warehouseId, string destination, Guid createdByUserId)
        : base(id)
    {
        WarehouseId = warehouseId;
        Destination = destination;
        CreatedByUserId = createdByUserId;
        Status = GoodsIssueStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid WarehouseId { get; private set; }

    public string Destination { get; private set; } = string.Empty;

    public Guid CreatedByUserId { get; private set; }

    public GoodsIssueStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? ApprovedAtUtc { get; private set; }

    public IReadOnlyCollection<GoodsIssueLine> Lines => _lines.AsReadOnly();

    public static GoodsIssue Create(Guid warehouseId, string destination, Guid createdByUserId)
    {
        Guard.AgainstEmpty(warehouseId, nameof(warehouseId));
        Guard.AgainstEmpty(createdByUserId, nameof(createdByUserId));

        return new GoodsIssue(Guid.CreateVersion7(), warehouseId, destination, createdByUserId);
    }

    public Result AddLine(Guid productId, decimal quantity)
    {
        if (Status != GoodsIssueStatus.Draft)
        {
            return Result.Failure(
                Error.Conflict("GoodsIssue.NotDraft", "Lines can only be added while the goods issue is in Draft status."));
        }

        _lines.Add(GoodsIssueLine.Create(Id, productId, quantity));

        return Result.Success();
    }

    public Result Approve()
    {
        if (Status != GoodsIssueStatus.Draft)
        {
            return Result.Failure(Error.Conflict("GoodsIssue.NotDraft", "Only a draft goods issue can be approved."));
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(
                Error.Validation("GoodsIssue.NoLines", "A goods issue must have at least one line before it can be approved."));
        }

        Status = GoodsIssueStatus.Approved;
        ApprovedAtUtc = DateTimeOffset.UtcNow;

        var lines = _lines.Select(line => new GoodsIssueApprovedLine(line.ProductId, line.Quantity)).ToList();
        AddDomainEvent(new GoodsIssueApprovedDomainEvent(Id, WarehouseId, lines, ApprovedAtUtc.Value));

        return Result.Success();
    }
}
