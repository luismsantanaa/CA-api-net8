using FluentValidation;

namespace Application.Features.Examples.Categories.Commands.Validators
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryValidator()
        {
            RuleFor(c => c.Name).NotEmpty().NotNull().MinimumLength(6).MaximumLength(150);
            RuleFor(c => c.Image).MaximumLength(300);
        }
    }
}
