using FluentValidation;

namespace WMS.Modules.Identity.Application.Users;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}
