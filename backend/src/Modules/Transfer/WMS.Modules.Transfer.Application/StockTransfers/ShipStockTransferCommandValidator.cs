using FluentValidation;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class ShipStockTransferCommandValidator : AbstractValidator<ShipStockTransferCommand>
{
    public ShipStockTransferCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
