using FluentValidation;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class GetStockMovementsQueryValidator : AbstractValidator<GetStockMovementsQuery>
{
    public GetStockMovementsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
