using FluentValidation;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class CreateGoodsIssueCommandValidator : AbstractValidator<CreateGoodsIssueCommand>
{
    public CreateGoodsIssueCommandValidator()
    {
        RuleFor(command => command.WarehouseId).NotEmpty();
        RuleFor(command => command.Destination).NotEmpty().MaximumLength(200);
        RuleFor(command => command.CreatedByUserId).NotEmpty();
        RuleFor(command => command.Lines).NotEmpty();

        RuleForEach(command => command.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
