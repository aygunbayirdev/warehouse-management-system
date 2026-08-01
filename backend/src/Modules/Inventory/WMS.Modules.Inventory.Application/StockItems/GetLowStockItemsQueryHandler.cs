using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class GetLowStockItemsQueryHandler(IStockItemReadRepository readRepository)
    : IQueryHandler<GetLowStockItemsQuery, IReadOnlyCollection<LowStockItemDto>>
{
    public async Task<Result<IReadOnlyCollection<LowStockItemDto>>> Handle(
        GetLowStockItemsQuery request,
        CancellationToken cancellationToken)
    {
        var lowStockItems = await readRepository.GetLowStockItemsAsync(request.Limit, cancellationToken);

        return Result.Success(lowStockItems);
    }
}
