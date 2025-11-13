using Application.DTOs;
using Application.Features.Examples.Products.Queries.Specs;
using Application.Features.Examples.Products.VMs;
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

namespace Application.Features.Examples.Products.Queries
{

    public class GetAllProductsByCategoryQuery : IRequest<Result<IReadOnlyList<ProductVm>>>
    {
        public string? CategoryId { get; set; }
        public bool? EnableCache { get; set; } = false;
    }

    public class GetAllProductsByCategoryQueryHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        IMapper _mapper,
        ILogger<GetAllProductsByCategoryQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetAllProductsByCategoryQuery, Result<IReadOnlyList<ProductVm>>>
    {
        private string _categoryId = string.Empty;

        public async Task<Result<IReadOnlyList<ProductVm>>> Handle(GetAllProductsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var result = new List<ProductVm>();

            _categoryId = request.CategoryId!;

            if (request.EnableCache == false)
            {
                result = await GetAllProductsByCategoryFromDb(cancellationToken);

                return Result<IReadOnlyList<ProductVm>>.Success(result, result.Count);
            }

            // Include CategoryId in cache key to ensure cache is specific per category
            // This prevents cache invalidation issues when products are updated
            var categoryIdGuid = request.CategoryId!.StringToGuid();
            var cacheKey = _cacheKeyService.GetKey($"{typeof(ProductVm).Name}:Category", categoryIdGuid);

            result = await _cacheService.GetOrSetAsync(cacheKey, () => GetAllProductsByCategoryFromDb(cancellationToken), cancellationToken: cancellationToken);

            return Result<IReadOnlyList<ProductVm>>.Success(result!, result!.Count);
        }

        private async Task<List<ProductVm>> GetAllProductsByCategoryFromDb(CancellationToken cancellationToken = default)
        {
            try
            {
                var repo = _repositoryFactory.GetRepository<TestProduct>();

                var spec = new GetProductsByCategory(_categoryId.StringToGuid());

                var data = await repo.GetAllWithSpec(spec, cancellationToken);
                ThrowException.Exception.IfClassNull(data);

                return _mapper.Map<List<ProductVm>>(data);
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
