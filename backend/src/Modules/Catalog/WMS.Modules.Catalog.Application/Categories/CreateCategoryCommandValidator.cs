using FluentValidation;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(100);
    }
}
