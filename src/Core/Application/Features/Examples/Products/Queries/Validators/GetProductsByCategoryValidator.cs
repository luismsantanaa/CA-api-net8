using Application.Commons;
using FluentValidation;

namespace Application.Features.Examples.Products.Queries.Validators
{
    public class GetProductsByCategoryValidator : AbstractValidator<GetAllProductsByCategoryQuery>
    {
        public GetProductsByCategoryValidator()
        {
            RuleFor(c => c.CategoryId).NotEmpty().NotNull().Must(ValidationHelper.IsGuid);
        }
    }
}
