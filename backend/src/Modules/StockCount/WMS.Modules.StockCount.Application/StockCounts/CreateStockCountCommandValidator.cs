using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class CreateStockCountCommandValidator : AbstractValidator<CreateStockCountCommand>
{
    public CreateStockCountCommandValidator()
    {
        RuleFor(command => command.WarehouseId).NotEmpty();
        RuleFor(command => command.CreatedByUserId).NotEmpty();
    }
}
