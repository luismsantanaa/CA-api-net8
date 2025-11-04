using Domain.Entities.Examples;
using FluentValidation;
using Persistence.Repositories.Contracts;

namespace Application.Features.Examples.Products.Commands.Validators
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        private readonly IRepositoryFactory _repositoryFactory;
        public CreateProductValidator(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;

            RuleFor(x => x.Name).NotEmpty().NotNull()
                .MinimumLength(10).MaximumLength(50);
            RuleFor(x => x.Description).NotEmpty().NotNull()
                .MinimumLength(10).MaximumLength(100); ;
            RuleFor(x => x.Price).NotEmpty().NotNull()
                .GreaterThan(0);
            RuleFor(x => x.Image).MinimumLength(10).MaximumLength(300);

            RuleFor(x => x.CategoryId).NotEmpty().NotNull()
                .MustAsync(CategoryIdExistInDb!)
                .WithMessage("La categoria insertada [{PropertyValue}] No Existe en la BD!");

        }

        private async Task<bool> CategoryIdExistInDb(Guid gui, CancellationToken cancellationToken)
        {
            try
            {
                var repo = _repositoryFactory.GetRepository<TestCategory>();
                return await repo.ExistsAsync(gui);
            }
            catch (Exception)
            {
                // If database error occurs during validation, return false to fail validation
                // The ValidationBehaviour will handle logging the exception
                return false;
            }
        }
    }
}
