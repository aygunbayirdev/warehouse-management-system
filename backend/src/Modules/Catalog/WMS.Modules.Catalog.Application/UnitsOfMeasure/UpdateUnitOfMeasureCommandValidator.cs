using FluentValidation;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed class UpdateUnitOfMeasureCommandValidator : AbstractValidator<UpdateUnitOfMeasureCommand>
{
    public UpdateUnitOfMeasureCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Code).NotEmpty().MaximumLength(10);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
    }
}
