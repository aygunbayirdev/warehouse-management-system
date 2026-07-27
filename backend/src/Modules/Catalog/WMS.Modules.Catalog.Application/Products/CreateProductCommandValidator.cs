using FluentValidation;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Sku).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.UnitOfMeasureId).NotEmpty();
        RuleFor(command => command.MinStockQuantity).GreaterThanOrEqualTo(0);
    }
}
