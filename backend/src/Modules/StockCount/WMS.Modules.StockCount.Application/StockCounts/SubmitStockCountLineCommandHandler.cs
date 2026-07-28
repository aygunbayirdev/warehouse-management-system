using MediatR;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Products;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class SubmitStockCountLineCommandHandler(IStockCountWriteRepository writeRepository, ISender sender)
    : ICommandHandler<SubmitStockCountLineCommand>
{
    public async Task<Result> Handle(SubmitStockCountLineCommand request, CancellationToken cancellationToken)
    {
        var stockCount = await writeRepository.GetByIdAsync(request.StockCountId, cancellationToken);

        if (stockCount is null)
        {
            return Result.Failure(Error.NotFound("StockCount.NotFound", "The stock count was not found."));
        }

        var productResult = await sender.Send(new GetProductByIdQuery(request.ProductId), cancellationToken);

        if (productResult.IsFailure)
        {
            return productResult;
        }

        var stockItemsResult = await sender.Send(
            new GetStockItemsQuery(stockCount.WarehouseId, request.ProductId),
            cancellationToken);

        var systemQuantity = stockItemsResult.Value.Select(stockItem => stockItem.Quantity).FirstOrDefault();

        var addLineResult = stockCount.AddLine(request.ProductId, systemQuantity, request.CountedQuantity);

        if (addLineResult.IsFailure)
        {
            return addLineResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
