using System.Linq.Expressions;
using Domain.Entities.Examples;
using Persistence.Specification;

namespace Application.Features.Examples.Categories.Queries.Specs
{
    internal class CategorySpecificationParams : SpecificationParams
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    internal class CategorySpecification : BaseSpecification<TestCategory>
    {
        public CategorySpecification(CategorySpecificationParams @params) : base(
            x =>
            (string.IsNullOrWhiteSpace(@params.Search) || x.Name!.Contains(@params.Search)) &&
            (string.IsNullOrWhiteSpace(@params.Description) || x.Description!.Contains(@params.Description)) &&
            (string.IsNullOrWhiteSpace(@params.Id) || x.Id.ToString().Contains(@params.Id)) &&
            (string.IsNullOrWhiteSpace(@params.Image) || x.Image!.Contains(@params.Image))
            )
        {
            // Use helper method to apply sorting with dictionary mapping
            var sortMappings = new Dictionary<string, Expression<Func<TestCategory, object>>>
            {
                { "idAsc", p => p.Id! },
                { "idDesc", p => p.Id! },
                { "namesAsc", p => p.Name! },
                { "namesDesc", p => p.Name! },
                { "descAsc", p => p.Description! },
                { "descDesc", p => p.Description! },
                { "imgAsc", p => p.Image! },
                { "imgDesc", p => p.Image! }
            };

            ApplySorting(@params.Sort, sortMappings, p => p.CreatedOn!, defaultOrderDescending: true);

            // Use helper method that accepts SpecificationParams directly
            ApplyPaging(@params);
        }
    }

    internal class CategoryForCountingSpecification : BaseSpecification<TestCategory>
    {
        public CategoryForCountingSpecification(CategorySpecificationParams @params) : base(
            x =>
            (string.IsNullOrWhiteSpace(@params.Search) || x.Name!.Contains(@params.Search)) &&
            (string.IsNullOrWhiteSpace(@params.Description) || x.Description!.Contains(@params.Description)) &&
            (string.IsNullOrWhiteSpace(@params.Id) || x.Id.ToString().Contains(@params.Id)) &&
            (string.IsNullOrWhiteSpace(@params.Image) || x.Image!.Contains(@params.Image))
            )
        { }
    }
}
