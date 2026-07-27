using FluentValidation;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(150);
        RuleFor(command => command.Address).MaximumLength(300);
    }
}
