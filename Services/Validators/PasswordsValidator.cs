using FluentValidation;

namespace ArticlesWebApp.Api.Services.Validators;

public class PasswordsValidator : AbstractValidator<string>
{
    public PasswordsValidator()
    {
        RuleFor(x => x)
            .NotEmpty()
            .MinimumLength(10).WithMessage("Password must be at least 10 symbols long.")
            .Must(s => s.Any(char.IsDigit)).WithMessage("Password must contain at least one number.")
            .Must(s => s.Any(char.IsUpper)).WithMessage("Password must contain at least one uppercase letter.")
            .Must(s => s.Any(char.IsLower)).WithMessage("Password must contain at least one lowercase letter.")
            .Must(s => s.Any(char.IsSymbol)).WithMessage("Password must contain at least one special symbol.");
    }
}