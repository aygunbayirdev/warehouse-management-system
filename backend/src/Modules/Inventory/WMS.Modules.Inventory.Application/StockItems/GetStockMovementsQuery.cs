using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed record GetStockMovementsQuery(
    Guid? WarehouseId,
    Guid? ProductId,
    DateTime? FromUtc,
    DateTime? ToUtc) : IQuery<IReadOnlyCollection<StockMovementDto>>;
