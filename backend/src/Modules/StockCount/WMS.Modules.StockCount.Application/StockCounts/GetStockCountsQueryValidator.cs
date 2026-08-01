using FluentValidation;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class GetStockCountsQueryValidator : AbstractValidator<GetStockCountsQuery>
{
    public GetStockCountsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
