using MediatR;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.StockItems;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class ShipStockTransferCommandHandler(IStockTransferWriteRepository writeRepository, ISender sender)
    : ICommandHandler<ShipStockTransferCommand>
{
    public async Task<Result> Handle(ShipStockTransferCommand request, CancellationToken cancellationToken)
    {
        var stockTransfer = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (stockTransfer is null)
        {
            return Result.Failure(Error.NotFound("StockTransfer.NotFound", "The stock transfer was not found."));
        }

        foreach (var line in stockTransfer.Lines)
        {
            var stockItemsResult = await sender.Send(
                new GetStockItemsQuery(stockTransfer.SourceWarehouseId, line.ProductId),
                cancellationToken);

            var availableQuantity = stockItemsResult.Value.Select(stockItem => stockItem.Quantity).FirstOrDefault();

            if (availableQuantity < line.Quantity)
            {
                return Result.Failure(
                    Error.Conflict("StockTransfer.InsufficientStock", $"Insufficient stock for product {line.ProductId} in the source warehouse."));
            }
        }

        var shipResult = stockTransfer.Ship();

        if (shipResult.IsFailure)
        {
            return shipResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
