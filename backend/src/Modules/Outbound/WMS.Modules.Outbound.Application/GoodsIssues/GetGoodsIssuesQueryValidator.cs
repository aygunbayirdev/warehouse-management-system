using FluentValidation;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class GetGoodsIssuesQueryValidator : AbstractValidator<GetGoodsIssuesQuery>
{
    public GetGoodsIssuesQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
    }
}
