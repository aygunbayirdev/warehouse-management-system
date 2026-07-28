using MediatR;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Products;
using WMS.Modules.Inventory.Application.Warehouses;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class CreateStockTransferCommandHandler(IStockTransferWriteRepository writeRepository, ISender sender)
    : ICommandHandler<CreateStockTransferCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var sourceWarehouseResult = await sender.Send(new GetWarehouseByIdQuery(request.SourceWarehouseId), cancellationToken);

        if (sourceWarehouseResult.IsFailure)
        {
            return Result.Failure<Guid>(sourceWarehouseResult.Error);
        }

        var destinationWarehouseResult = await sender.Send(new GetWarehouseByIdQuery(request.DestinationWarehouseId), cancellationToken);

        if (destinationWarehouseResult.IsFailure)
        {
            return Result.Failure<Guid>(destinationWarehouseResult.Error);
        }

        var createResult = StockTransfer.Create(request.SourceWarehouseId, request.DestinationWarehouseId, request.CreatedByUserId);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        var stockTransfer = createResult.Value;

        foreach (var line in request.Lines)
        {
            var productResult = await sender.Send(new GetProductByIdQuery(line.ProductId), cancellationToken);

            if (productResult.IsFailure)
            {
                return Result.Failure<Guid>(productResult.Error);
            }

            var addLineResult = stockTransfer.AddLine(line.ProductId, line.Quantity);

            if (addLineResult.IsFailure)
            {
                return Result.Failure<Guid>(addLineResult.Error);
            }
        }

        writeRepository.Add(stockTransfer);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return stockTransfer.Id;
    }
}
