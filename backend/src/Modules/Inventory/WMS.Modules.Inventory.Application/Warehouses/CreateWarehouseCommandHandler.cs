using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed class CreateWarehouseCommandHandler(IWarehouseWriteRepository writeRepository)
    : ICommandHandler<CreateWarehouseCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var existing = await writeRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (existing is not null)
        {
            return Result.Failure<Guid>(
                Error.Conflict("Warehouse.CodeAlreadyExists", $"A warehouse with code '{request.Code}' already exists."));
        }

        var warehouse = Warehouse.Create(request.Code, request.Name, request.Address);

        writeRepository.Add(warehouse);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return warehouse.Id;
    }
}
