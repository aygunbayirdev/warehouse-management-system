using FluentValidation;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class ApproveGoodsIssueCommandValidator : AbstractValidator<ApproveGoodsIssueCommand>
{
    public ApproveGoodsIssueCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
