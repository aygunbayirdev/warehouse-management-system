using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class SubmitStockCountLineCommandValidator : AbstractValidator<SubmitStockCountLineCommand>
{
    public SubmitStockCountLineCommandValidator()
    {
        RuleFor(command => command.StockCountId).NotEmpty();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.CountedQuantity).GreaterThanOrEqualTo(0);
    }
}
