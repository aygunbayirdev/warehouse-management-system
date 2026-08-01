using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed record GetLowStockItemsQuery(int Limit) : IQuery<IReadOnlyCollection<LowStockItemDto>>;
