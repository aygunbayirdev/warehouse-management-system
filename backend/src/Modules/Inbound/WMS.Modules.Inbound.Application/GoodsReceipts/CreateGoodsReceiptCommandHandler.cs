using MediatR;
using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Products;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Domain;
using WMS.Modules.Inventory.Application.Warehouses;
using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class CreateGoodsReceiptCommandHandler(IGoodsReceiptWriteRepository writeRepository, ISender sender)
    : ICommandHandler<CreateGoodsReceiptCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var warehouseResult = await sender.Send(new GetWarehouseByIdQuery(request.WarehouseId), cancellationToken);

        if (warehouseResult.IsFailure)
        {
            return Result.Failure<Guid>(warehouseResult.Error);
        }

        var goodsReceipt = GoodsReceipt.Create(request.WarehouseId, request.CreatedByUserId);

        foreach (var line in request.Lines)
        {
            var productResult = await sender.Send(new GetProductByIdQuery(line.ProductId), cancellationToken);

            if (productResult.IsFailure)
            {
                return Result.Failure<Guid>(productResult.Error);
            }

            var addLineResult = goodsReceipt.AddLine(line.ProductId, line.Quantity);

            if (addLineResult.IsFailure)
            {
                return Result.Failure<Guid>(addLineResult.Error);
            }
        }

        writeRepository.Add(goodsReceipt);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return goodsReceipt.Id;
    }
}
