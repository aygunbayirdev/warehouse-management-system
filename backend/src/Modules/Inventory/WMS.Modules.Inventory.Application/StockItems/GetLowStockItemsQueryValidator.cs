using FluentValidation;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class GetLowStockItemsQueryValidator : AbstractValidator<GetLowStockItemsQuery>
{
    public GetLowStockItemsQueryValidator()
    {
        RuleFor(query => query.Limit).InclusiveBetween(1, 50);
    }
}
