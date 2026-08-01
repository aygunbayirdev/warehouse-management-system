using FluentValidation;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class GetStockTransfersQueryValidator : AbstractValidator<GetStockTransfersQuery>
{
    public GetStockTransfersQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
