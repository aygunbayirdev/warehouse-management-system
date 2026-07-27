using FluentValidation;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class CreateGoodsReceiptCommandValidator : AbstractValidator<CreateGoodsReceiptCommand>
{
    public CreateGoodsReceiptCommandValidator()
    {
        RuleFor(command => command.WarehouseId).NotEmpty();
        RuleFor(command => command.CreatedByUserId).NotEmpty();
        RuleFor(command => command.Lines).NotEmpty();

        RuleForEach(command => command.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
