using FluentValidation;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class ApproveGoodsReceiptCommandValidator : AbstractValidator<ApproveGoodsReceiptCommand>
{
    public ApproveGoodsReceiptCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
