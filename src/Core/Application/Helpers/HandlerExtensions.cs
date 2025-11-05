using Domain.Base;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Shared.Extensions.Contracts;

namespace Application.Helpers
{
    /// <summary>
    /// Extension methods for common handler operations.
    /// Simplifies repetitive code in handlers.
    /// </summary>
    public static class HandlerExtensions
    {
        /// <summary>
        /// Gets an entity by ID from the repository factory.
        /// Throws NotFoundException if the entity doesn't exist.
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="repositoryFactory">The repository factory</param>
        /// <param name="id">The entity ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity if found</returns>
        /// <exception cref="NotFoundException">Thrown when entity is not found</exception>
        public static async Task<TEntity> GetEntityByIdOrThrowAsync<TEntity>(
            this IRepositoryFactory repositoryFactory,
            Guid id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity, new()
        {
            var repo = repositoryFactory.GetRepository<TEntity>();
            var entity = await repo.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                var entityName = typeof(TEntity).Name;
                throw new NotFoundException($"No se encontró el registro con ID {id} en {entityName}");
            }

            return entity!; // Safe to return here as we've checked for null
        }

        /// <summary>
        /// Checks if an entity exists by ID.
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="repositoryFactory">The repository factory</param>
        /// <param name="id">The entity ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if entity exists, false otherwise</returns>
        public static async Task<bool> EntityExistsAsync<TEntity>(
            this IRepositoryFactory repositoryFactory,
            Guid id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity, new()
        {
            var repo = repositoryFactory.GetRepository<TEntity>();
            return await repo.ExistsAsync(id, cancellationToken);
        }

        /// <summary>
        /// Gets an entity by ID from UnitOfWork repository.
        /// Throws NotFoundException if the entity doesn't exist.
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="id">The entity ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The entity if found</returns>
        /// <exception cref="NotFoundException">Thrown when entity is not found</exception>
        public static async Task<TEntity> GetEntityByIdOrThrowAsync<TEntity>(
            this IUnitOfWork unitOfWork,
            Guid id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity, new()
        {
            var repo = unitOfWork.Repository<TEntity>();
            var entity = await repo.GetByIdAsync(id, cancellationToken);

            if (entity == null)
            {
                var entityName = typeof(TEntity).Name;
                throw new NotFoundException($"No se encontró el registro con ID {id} en {entityName}");
            }

            return entity!; // Safe to return here as we've checked for null
        }

        /// <summary>
        /// Checks if an entity exists by ID using UnitOfWork.
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="unitOfWork">The unit of work</param>
        /// <param name="id">The entity ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if entity exists, false otherwise</returns>
        public static async Task<bool> EntityExistsAsync<TEntity>(
            this IUnitOfWork unitOfWork,
            Guid id,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity, new()
        {
            var repo = unitOfWork.Repository<TEntity>();
            return await repo.ExistsAsync(id, cancellationToken);
        }
    }
}

