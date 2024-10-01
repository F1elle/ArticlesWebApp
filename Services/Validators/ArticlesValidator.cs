using ArticlesWebApp.Api.DTOs;
using FluentValidation;

namespace ArticlesWebApp.Api.Services.Validators;

public class ArticlesValidator : AbstractValidator<InputArticlesDto>
{
    public ArticlesValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .NotNull()
            .MinimumLength(5)
            .MaximumLength(120);
        RuleFor(x => x.Content)
            .NotEmpty()
            .NotNull()
            .MinimumLength(200)
            .MaximumLength(25000);
    }
}