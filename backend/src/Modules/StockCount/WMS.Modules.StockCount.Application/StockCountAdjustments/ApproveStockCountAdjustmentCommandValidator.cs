using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class ApproveStockCountAdjustmentCommandValidator : AbstractValidator<ApproveStockCountAdjustmentCommand>
{
    public ApproveStockCountAdjustmentCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.ApprovedByUserId).NotEmpty();
    }
}
