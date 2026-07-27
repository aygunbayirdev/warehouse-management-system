using FluentValidation;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed class CreateUnitOfMeasureCommandValidator : AbstractValidator<CreateUnitOfMeasureCommand>
{
    public CreateUnitOfMeasureCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty().MaximumLength(10);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
    }
}
