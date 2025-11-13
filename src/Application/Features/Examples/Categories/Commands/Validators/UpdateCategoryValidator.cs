using Application.Commons;
using FluentValidation;

namespace Application.Features.Examples.Categories.Commands.Validators
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(c => c.Id).NotEmpty().NotNull().Must(ValidationHelper.IsGuid);
            RuleFor(c => c.Name).MinimumLength(6).MaximumLength(150);
            RuleFor(c => c.Image).MaximumLength(300);
        }
    }
}
