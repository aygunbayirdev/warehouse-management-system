using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed record GetStockItemsQuery(Guid? WarehouseId, Guid? ProductId) : IQuery<IReadOnlyCollection<StockItemDto>>;
