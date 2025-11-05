using AutoMapper;
using Domain.Base;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Pagination;
using Persistence.Repositories.Contracts;
using Persistence.Specification;
using Persistence.Specification.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Handlers.Base
{
    /// <summary>
    /// Base handler for paginated queries that reduces boilerplate code.
    /// Handles common pagination logic including data retrieval, counting, and PaginationVm creation.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TVm">The view model type</typeparam>
    /// <typeparam name="TParams">The specification parameters type</typeparam>
    /// <typeparam name="TRequest">The request type that must implement IRequest&lt;PaginationVm&lt;TVm&gt;&gt;</typeparam>
    public abstract class PaginatedQueryHandlerBase<TEntity, TVm, TParams, TRequest> 
        : IRequestHandler<TRequest, PaginationVm<TVm>>
        where TEntity : BaseEntity, new()
        where TVm : class
        where TParams : SpecificationParams
        where TRequest : IRequest<PaginationVm<TVm>>
    {
        protected readonly IMapper Mapper;
        protected readonly ILogger Logger;
        protected readonly IRepositoryFactory RepositoryFactory;

        protected PaginatedQueryHandlerBase(
            IMapper mapper,
            ILogger logger,
            IRepositoryFactory repositoryFactory)
        {
            Mapper = mapper;
            Logger = logger;
            RepositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Creates the specification for data retrieval (with pagination).
        /// </summary>
        /// <param name="params">The specification parameters</param>
        /// <returns>Specification with pagination applied</returns>
        protected abstract ISpecification<TEntity> CreateSpecification(TParams @params);

        /// <summary>
        /// Creates the specification for counting records (without pagination).
        /// </summary>
        /// <param name="params">The specification parameters</param>
        /// <returns>Specification without pagination for counting</returns>
        protected abstract ISpecification<TEntity> CreateCountingSpecification(TParams @params);

        /// <summary>
        /// Creates specification parameters from the request.
        /// </summary>
        /// <param name="request">The paginated query request</param>
        /// <returns>Specification parameters</returns>
        protected abstract TParams CreateParamsFromRequest(TRequest request);

        /// <summary>
        /// Handles the paginated query request.
        /// </summary>
        /// <param name="request">The paginated query request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PaginationVm with paginated data</returns>
        public virtual async Task<PaginationVm<TVm>> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var specificationParams = CreateParamsFromRequest(request);
                var repo = RepositoryFactory.GetRepository<TEntity>();

                // Get paginated data
                var spec = CreateSpecification(specificationParams);
                var resultData = await repo.GetAllWithSpec(spec, cancellationToken);
                ThrowException.Exception.IfClassNull(resultData);

                // Get total count (without pagination)
                var specCount = CreateCountingSpecification(specificationParams);
                var totalRecords = await repo.CountAsync(specCount, cancellationToken);

                // Map entities to view models
                var vmData = Mapper.Map<IReadOnlyList<TEntity>, IReadOnlyList<TVm>>(resultData);

                // Create and return pagination view model
                return Helpers.PaginationVmHelper.Create(totalRecords, specificationParams, vmData);
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex);
                Logger.LogError(ex, "Error in paginated query handler. Error: {ErrorMessage}", message);
                throw new InternalServerError(message, ex);
            }
        }
    }
}

