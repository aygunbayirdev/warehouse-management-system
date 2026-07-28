using FluentValidation;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class ReceiveStockTransferCommandValidator : AbstractValidator<ReceiveStockTransferCommand>
{
    public ReceiveStockTransferCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
