using Application.Commons;
using Application.Features.Examples.Products.Queries;
using FluentValidation;

namespace Application.Features.Examples.Products.Queries.Validators
{
    public class GetProductByIdValidator : AbstractValidator<GetProductByIdQuery>
    {
        public GetProductByIdValidator()
        {
            RuleFor(c => c.PkId).NotEmpty().NotNull().Must(ValidationHelper.IsGuid);
        }
    }
}
