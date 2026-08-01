using FluentValidation;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class GetGoodsReceiptsQueryValidator : AbstractValidator<GetGoodsReceiptsQuery>
{
    public GetGoodsReceiptsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
