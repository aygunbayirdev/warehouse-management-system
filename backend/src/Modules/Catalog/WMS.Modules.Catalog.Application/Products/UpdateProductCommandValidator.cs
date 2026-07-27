using FluentValidation;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.UnitOfMeasureId).NotEmpty();
        RuleFor(command => command.MinStockQuantity).GreaterThanOrEqualTo(0);
    }
}
