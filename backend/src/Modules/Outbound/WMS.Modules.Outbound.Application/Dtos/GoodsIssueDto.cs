namespace WMS.Modules.Outbound.Application.Dtos;

public sealed record GoodsIssueDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    string Destination,
    string Status,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime? ApprovedAtUtc,
    IReadOnlyCollection<GoodsIssueLineDto> Lines);
