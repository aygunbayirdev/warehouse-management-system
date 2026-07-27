using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class GetStockItemsQueryHandler(IStockItemReadRepository readRepository)
    : IQueryHandler<GetStockItemsQuery, IReadOnlyCollection<StockItemDto>>
{
    public async Task<Result<IReadOnlyCollection<StockItemDto>>> Handle(
        GetStockItemsQuery request,
        CancellationToken cancellationToken)
    {
        var stockItems = await readRepository.GetListAsync(request.WarehouseId, request.ProductId, cancellationToken);

        return Result.Success(stockItems);
    }
}
