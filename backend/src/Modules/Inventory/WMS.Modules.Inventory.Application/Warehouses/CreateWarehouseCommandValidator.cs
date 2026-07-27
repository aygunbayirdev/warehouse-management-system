using FluentValidation;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty().MaximumLength(20);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.Address).MaximumLength(300);
    }
}
