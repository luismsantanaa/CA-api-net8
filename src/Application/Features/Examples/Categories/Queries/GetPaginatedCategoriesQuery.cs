using Application.Features.Examples.Categories.Queries.Specs;
using Application.Features.Examples.Categories.VMs;
using Application.Handlers.Base;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Pagination;
using Persistence.Repositories.Contracts;
using Persistence.Specification.Contracts;

namespace Application.Features.Examples.Categories.Queries
{
    public class GetPaginatedCategoriesQuery : PaginationBase, IRequest<PaginationVm<CategoryVm>>
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
    }

    internal class GetPaginatedCategoriesQueryHandler(
        IMapper mapper,
        ILogger<GetPaginatedCategoriesQueryHandler> logger,
        IRepositoryFactory repositoryFactory)
        : PaginatedQueryHandlerBase<TestCategory, CategoryVm, CategorySpecificationParams, GetPaginatedCategoriesQuery>(
            mapper, logger, repositoryFactory)
    {
        protected override ISpecification<TestCategory> CreateSpecification(CategorySpecificationParams @params)
        {
            return new CategorySpecification(@params);
        }

        protected override ISpecification<TestCategory> CreateCountingSpecification(CategorySpecificationParams @params)
        {
            return new CategoryForCountingSpecification(@params);
        }

        protected override CategorySpecificationParams CreateParamsFromRequest(GetPaginatedCategoriesQuery request)
        {
            return new CategorySpecificationParams
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Search = request.Search,
                Sort = request.Sort,
                Id = request.Id,
                Image = request.Image
            };
        }
    }
}
