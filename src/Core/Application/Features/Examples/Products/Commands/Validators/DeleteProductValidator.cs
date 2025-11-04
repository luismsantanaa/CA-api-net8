using Application.Commons;
using FluentValidation;

namespace Application.Features.Examples.Products.Commands.Validators
{
    public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
    {
        public DeleteProductValidator()
        {
            RuleFor(c => c.Id).NotEmpty().NotNull().Must(ValidationHelper.IsGuid);
        }
    }
}
