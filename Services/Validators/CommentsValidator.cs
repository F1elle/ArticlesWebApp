using ArticlesWebApp.Api.DTOs;
using FluentValidation;

namespace ArticlesWebApp.Api.Services.Validators;

public class CommentsValidator : AbstractValidator<InputCommentsDto>
{
    public CommentsValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .NotNull()
            .MinimumLength(3)
            .MaximumLength(500);
    }
}