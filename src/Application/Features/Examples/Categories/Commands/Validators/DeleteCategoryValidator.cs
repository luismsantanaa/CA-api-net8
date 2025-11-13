using Application.Commons;
using FluentValidation;

namespace Application.Features.Examples.Categories.Commands.Validators
{
    public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryValidator()
        {
            RuleFor(c => c.Id).NotEmpty().NotNull().Must(ValidationHelper.IsGuid);
        }
    }
}
