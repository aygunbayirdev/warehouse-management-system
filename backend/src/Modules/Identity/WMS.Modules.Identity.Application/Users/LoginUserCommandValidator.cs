using FluentValidation;

namespace WMS.Modules.Identity.Application.Users;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotEmpty();
    }
}
