using Application.Commons;
using FluentValidation;

namespace Application.Features.Examples.Products.Queries.Validators
{
    internal class GetProductWithCategoryValidator : AbstractValidator<GetProductWithCategoryByIdQuery>
    {
        public GetProductWithCategoryValidator()
        {
            RuleFor(c => c.Id).NotEmpty().NotNull().Must(ValidationHelper.IsGuid);
        }
    }
}
