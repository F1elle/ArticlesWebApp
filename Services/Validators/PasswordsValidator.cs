using FluentValidation;

namespace ArticlesWebApp.Api.Services.Validators;

public class PasswordsValidator : AbstractValidator<string>
{
    public PasswordsValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 symbols long.")
            .Must(s => s.Any(char.IsDigit)).WithMessage("Password must contain at least one number.");
    }
}