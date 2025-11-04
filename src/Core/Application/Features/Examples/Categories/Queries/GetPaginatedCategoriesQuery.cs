using Application.Features.Examples.Categories.Queries.Specs;
using Application.Features.Examples.Categories.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Pagination;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Examples.Categories.Queries
{
    public class GetPaginatedCategoriesQuery : PaginationBase, IRequest<PaginationVm<CategoryVm>>
    {
        public string? Id { get; set; }
        public string? Image { get; set; }
    }

    public class GetPaginatedCategoriesQueryHandler(
        IMapper _mapper,
        ILogger<GetPaginatedCategoriesQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetPaginatedCategoriesQuery, PaginationVm<CategoryVm>>
    {

        public async Task<PaginationVm<CategoryVm>> Handle(GetPaginatedCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var specificationParams = new CategorySpecificationParams
                {
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize,
                    Search = request.Search,
                    Sort = request.Sort,
                    Id = request.Id,
                    Image = request.Image,
                };

                var repo = _repositoryFactory.GetRepository<TestCategory>();

                var spec = new CategorySpecification(specificationParams);

                var resultData = await repo.GetAllWithSpec(spec, cancellationToken);
                ThrowException.Exception.IfClassNull(resultData);

                var spectCount = new CategoryForCountingSpecification(specificationParams);
                var totalRecords = await repo.CountAsync(spectCount, cancellationToken);

                var rounded = Math.Ceiling(Convert.ToDecimal(totalRecords) / Convert.ToDecimal(specificationParams.PageSize));
                var totalPages = Convert.ToInt32(rounded);

                var data = _mapper.Map<IReadOnlyList<TestCategory>, IReadOnlyList<CategoryVm>>(resultData);

                var pagination = new PaginationVm<CategoryVm>
                {
                    Count = totalRecords,
                    Data = data,
                    PageCount = totalPages,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize
                };

                return pagination;
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex);
                _logger.LogError(ex, message);
                throw new InternalServerError(message, ex);
            }
        }
    }
}
