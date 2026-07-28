using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class RejectStockCountAdjustmentCommandValidator : AbstractValidator<RejectStockCountAdjustmentCommand>
{
    public RejectStockCountAdjustmentCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.ApprovedByUserId).NotEmpty();
    }
}
