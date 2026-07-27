using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed class UpdateWarehouseCommandHandler(IWarehouseWriteRepository writeRepository)
    : ICommandHandler<UpdateWarehouseCommand>
{
    public async Task<Result> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (warehouse is null)
        {
            return Result.Failure(Error.NotFound("Warehouse.NotFound", "The warehouse was not found."));
        }

        warehouse.Update(request.Name, request.Address);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
