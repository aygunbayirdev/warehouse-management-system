using FluentValidation;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class DecreaseStockCommandValidator : AbstractValidator<DecreaseStockCommand>
{
    public DecreaseStockCommandValidator()
    {
        RuleFor(command => command.SourceEventId).NotEmpty();
        RuleFor(command => command.LineNumber).GreaterThanOrEqualTo(0);
        RuleFor(command => command.WarehouseId).NotEmpty();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0);
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(200);
    }
}
