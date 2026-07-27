using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed class DeleteWarehouseCommandHandler(
    IWarehouseWriteRepository writeRepository,
    IStockItemWriteRepository stockItemWriteRepository)
    : ICommandHandler<DeleteWarehouseCommand>
{
    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (warehouse is null)
        {
            return Result.Failure(Error.NotFound("Warehouse.NotFound", "The warehouse was not found."));
        }

        var hasStock = await stockItemWriteRepository.ExistsWithWarehouseIdAsync(request.Id, cancellationToken);

        if (hasStock)
        {
            return Result.Failure(
                Error.Conflict("Warehouse.InUse", "The warehouse has stock records and cannot be deleted."));
        }

        writeRepository.Remove(warehouse);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
