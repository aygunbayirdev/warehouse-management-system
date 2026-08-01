using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class GetStockCountAdjustmentsQueryValidator : AbstractValidator<GetStockCountAdjustmentsQuery>
{
    public GetStockCountAdjustmentsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
