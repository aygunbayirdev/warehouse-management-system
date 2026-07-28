using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class StartStockCountCommandValidator : AbstractValidator<StartStockCountCommand>
{
    public StartStockCountCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
