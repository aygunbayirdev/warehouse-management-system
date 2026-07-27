using FluentValidation;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class IncreaseStockCommandValidator : AbstractValidator<IncreaseStockCommand>
{
    public IncreaseStockCommandValidator()
    {
        RuleFor(command => command.WarehouseId).NotEmpty();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(200);
    }
}
