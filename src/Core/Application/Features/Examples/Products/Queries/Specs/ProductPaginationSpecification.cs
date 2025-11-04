using System.Linq.Expressions;
using Domain.Entities.Examples;
using Persistence.Specification;

namespace Application.Features.Examples.Products.Queries.Specs
{
    internal class ProductSpecificationParams : SpecificationParams
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
    }

    internal class ProductSpecification : BaseSpecification<TestProduct>
    {
        public ProductSpecification(ProductSpecificationParams @params) : base(
            x =>
             (string.IsNullOrWhiteSpace(@params.Search) || x.Name!.Contains(@params.Search!)) &&
             (string.IsNullOrWhiteSpace(@params.Description) || x.Description!.Contains(@params.Description!)) &&
             (string.IsNullOrWhiteSpace(@params.CategoryName) || x.Category!.Name!.Contains(@params.CategoryName!))
            )
        {
            AddInclude(x => x.Category!);

            // Use helper method to apply sorting with dictionary mapping
            var sortMappings = new Dictionary<string, Expression<Func<TestProduct, object>>>
            {
                { "titleAsc", p => p.Name! },
                { "titleDesc", p => p.Name! },
                { "priceAsc", p => p.Price! },
                { "priceDesc", p => p.Price! },
                { "categoryAsc", p => p.Category!.Name! },
                { "categoryDesc", p => p.Category!.Name! }
            };

            ApplySorting(@params.Sort, sortMappings, p => p.CreatedOn!, defaultOrderDescending: true);

            // Use helper method that accepts SpecificationParams directly
            ApplyPaging(@params);
        }
    }

    internal class ProductForCountingSpecification : BaseSpecification<TestProduct>
    {
        public ProductForCountingSpecification(ProductSpecificationParams @params) : base(
            x =>
             (string.IsNullOrWhiteSpace(@params.Search) || x.Name!.Contains(@params.Search!)) &&
             (string.IsNullOrWhiteSpace(@params.Description) || x.Description!.Contains(@params.Search!)) &&
             (string.IsNullOrWhiteSpace(@params.CategoryName) || x.Category!.Name!.Contains(@params.CategoryName!))
            )
        {
            Includes.Add(x => x.Category!);
        }
    }
}
