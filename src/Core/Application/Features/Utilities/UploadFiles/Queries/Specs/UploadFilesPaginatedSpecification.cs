using System.Linq.Expressions;
using Domain.Entities.Shared;
using Persistence.Specification;

namespace Application.Features.Utilities.UploadFiles.Queries.Specs
{
    public class UploadFilesSpecificationParams : SpecificationParams
    { }

    public class UploadFilesSpecification : BaseSpecification<UploadedFile>
    {
        public UploadFilesSpecification(UploadFilesSpecificationParams @params) : base(
                x => string.IsNullOrEmpty(@params.Search) || x.Name!.Contains(@params.Search)
            )
        {
            // Use helper method to apply sorting with dictionary mapping
            var sortMappings = new Dictionary<string, Expression<Func<UploadedFile, object>>>
            {
                { "nameAsc", p => p.Name! },
                { "nameDesc", p => p.Name! }
            };

            ApplySorting(@params.Sort, sortMappings, p => p.CreatedOn!, defaultOrderDescending: false);

            // Use helper method that accepts SpecificationParams directly
            ApplyPaging(@params);
        }

    }

    public class UploadFilesForCountingSpecification : BaseSpecification<UploadedFile>
    {
        public UploadFilesForCountingSpecification(UploadFilesSpecificationParams @params) : base(
                x => string.IsNullOrEmpty(@params.Search) || x.Name!.Contains(@params.Search)
            )
        { }
    }
}
