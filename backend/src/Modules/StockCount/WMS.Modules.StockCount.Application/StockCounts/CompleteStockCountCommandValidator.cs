using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class CompleteStockCountCommandValidator : AbstractValidator<CompleteStockCountCommand>
{
    public CompleteStockCountCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
