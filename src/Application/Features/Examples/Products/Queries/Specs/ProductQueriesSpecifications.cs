using Domain.Entities.Examples;
using Persistence.Specification;

namespace Application.Features.Examples.Products.Queries.Specs
{
    internal class GetAllProductsWithCategoryDescription : BaseSpecification<TestProduct>
    {
        public GetAllProductsWithCategoryDescription()
        {
            AddInclude(x => x.Category!);
            AddOrderByDescending(x => x.CreatedOn);
        }
    }

    internal class GetProductsById : BaseSpecification<TestProduct>
    {
        public GetProductsById(Guid id) : base(x => x.Id == id)
        { }
    }

    internal class GetProductsByCategory : BaseSpecification<TestProduct>
    {
        public GetProductsByCategory(Guid catId) : base(x => x.CategoryId == catId)
        {
            AddInclude(x => x.Category!);
            AddOrderBy(x => x.Category!.Name!);
        }
    }

    internal class GetProductWithCategory : BaseSpecification<TestProduct>
    {
        public GetProductWithCategory(Guid id) : base(x => x.Id == id)
        {
            AddInclude(x => x.Category!);
        }
    }
}
