using Application.Commons;
using Application.Features.Examples.Categories.Queries;
using FluentValidation;

namespace Application.Features.Examples.Categories.Queries.Validators
{
    public class GetCategoryByIdValidator : AbstractValidator<GetCategoryByIdQuery>
    {
        public GetCategoryByIdValidator()
        {
            RuleFor(c => c.PkId).NotEmpty().NotNull().Must(ValidationHelper.IsGuid);
        }
    }
}

