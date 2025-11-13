using Application.DTOs;
using Application.Features.Examples.Categories.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Persistence.Caching.Extensions;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Examples.Categories.Queries
{

    public class GetAllCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryVm>>>
    {
        public bool? EnableCache { get; set; } = false;
    }

    public class GetAllCategoriesQueryHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        IMapper _mapper,
        ILogger<GetAllCategoriesQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryVm>>>
    {

        public async Task<Result<IReadOnlyList<CategoryVm>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var result = new List<CategoryVm>();

            if (request.EnableCache == false)
            {
                result = await GetAllCategoryFromDb(cancellationToken);

                return Result<IReadOnlyList<CategoryVm>>.Success(result, result.Count);
            }

            var cacheKey = _cacheKeyService.GetListKey(typeof(CategoryVm).Name);

            result = await _cacheService.GetOrSetAsync(cacheKey, () => GetAllCategoryFromDb(cancellationToken), cancellationToken: cancellationToken);

            return Result<IReadOnlyList<CategoryVm>>.Success(result!, result!.Count);
        }

        private async Task<List<CategoryVm>> GetAllCategoryFromDb(CancellationToken cancellationToken = default)
        {
            try
            {
                var repo = _repositoryFactory.GetRepository<TestCategory>();

                var data = await repo.GetAllAsync(cancellationToken);
                ThrowException.Exception.IfClassNull(data);

                return _mapper.Map<List<CategoryVm>>(data);
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
