using MediatR;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Products;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Inventory.Application.Warehouses;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class CreateGoodsIssueCommandHandler(IGoodsIssueWriteRepository writeRepository, ISender sender)
    : ICommandHandler<CreateGoodsIssueCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var warehouseResult = await sender.Send(new GetWarehouseByIdQuery(request.WarehouseId), cancellationToken);

        if (warehouseResult.IsFailure)
        {
            return Result.Failure<Guid>(warehouseResult.Error);
        }

        var goodsIssue = GoodsIssue.Create(request.WarehouseId, request.Destination, request.CreatedByUserId);

        foreach (var line in request.Lines)
        {
            var productResult = await sender.Send(new GetProductByIdQuery(line.ProductId), cancellationToken);

            if (productResult.IsFailure)
            {
                return Result.Failure<Guid>(productResult.Error);
            }

            var stockItemsResult = await sender.Send(
                new GetStockItemsQuery(request.WarehouseId, line.ProductId),
                cancellationToken);

            var availableQuantity = stockItemsResult.Value.Select(stockItem => stockItem.Quantity).FirstOrDefault();

            if (availableQuantity < line.Quantity)
            {
                return Result.Failure<Guid>(
                    Error.Conflict("GoodsIssue.InsufficientStock", $"Insufficient stock for product {line.ProductId} in the selected warehouse."));
            }

            var addLineResult = goodsIssue.AddLine(line.ProductId, line.Quantity);

            if (addLineResult.IsFailure)
            {
                return Result.Failure<Guid>(addLineResult.Error);
            }
        }

        writeRepository.Add(goodsIssue);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return goodsIssue.Id;
    }
}
