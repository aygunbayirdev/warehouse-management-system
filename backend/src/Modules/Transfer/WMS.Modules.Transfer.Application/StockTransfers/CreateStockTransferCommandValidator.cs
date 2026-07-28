using FluentValidation;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class CreateStockTransferCommandValidator : AbstractValidator<CreateStockTransferCommand>
{
    public CreateStockTransferCommandValidator()
    {
        RuleFor(command => command.SourceWarehouseId).NotEmpty();
        RuleFor(command => command.DestinationWarehouseId).NotEmpty();
        RuleFor(command => command.CreatedByUserId).NotEmpty();
        RuleFor(command => command.Lines).NotEmpty();

        RuleForEach(command => command.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
