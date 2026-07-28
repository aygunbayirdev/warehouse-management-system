using MediatR;
using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Warehouses;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class CreateStockCountCommandHandler(IStockCountWriteRepository writeRepository, ISender sender)
    : ICommandHandler<CreateStockCountCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateStockCountCommand request, CancellationToken cancellationToken)
    {
        var warehouseResult = await sender.Send(new GetWarehouseByIdQuery(request.WarehouseId), cancellationToken);

        if (warehouseResult.IsFailure)
        {
            return Result.Failure<Guid>(warehouseResult.Error);
        }

        var stockCount = StockCountAggregate.Create(request.WarehouseId, request.CreatedByUserId);

        writeRepository.Add(stockCount);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return stockCount.Id;
    }
}
