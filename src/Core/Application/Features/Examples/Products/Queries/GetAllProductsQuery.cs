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
    /// <summary>
    /// Query to retrieve all products from the system
    /// </summary>
    public class GetAllProductsQuery : IRequest<Result<IReadOnlyList<ProductVm>>>
    {
        /// <summary>
        /// Whether to use cache for retrieving products. Defaults to false.
        /// </summary>
        public bool? EnableCache { get; set; } = false;
    }

    /// <summary>
    /// Handler for GetAllProductsQuery.
    /// Retrieves all products with their category descriptions, optionally using cache.
    /// </summary>
    public class GetAllProductsQueryHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        IMapper _mapper,
        ILogger<GetAllProductsQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<ProductVm>>>
    {

        public async Task<Result<IReadOnlyList<ProductVm>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var result = new List<ProductVm>();

            if (request.EnableCache == false)
            {
                result = await GetAllProductFromDb(cancellationToken);

                return Result<IReadOnlyList<ProductVm>>.Success(result, result.Count);
            }

            var cacheKey = _cacheKeyService.GetListKey(typeof(ProductVm).Name);

            result = await _cacheService.GetOrSetAsync(cacheKey, () => GetAllProductFromDb(cancellationToken), cancellationToken: cancellationToken);

            return Result<IReadOnlyList<ProductVm>>.Success(result!, result!.Count);
        }

        private async Task<List<ProductVm>> GetAllProductFromDb(CancellationToken cancellationToken = default)
        {
            try
            {
                var repo = _repositoryFactory.GetRepository<TestProduct>();

                var spec = new GetAllProductsWithCategoryDescription();

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
