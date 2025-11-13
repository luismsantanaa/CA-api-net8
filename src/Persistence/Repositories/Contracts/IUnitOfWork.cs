using Domain.Base;
using Persistence.Repositories.Contracts;

namespace Persistence.Repositories.Contracts
{
    /// <summary>
    /// Unit of Work pattern interface for coordinating database operations across multiple repositories.
    /// Ensures that all repository operations are committed as a single transaction.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Gets a repository for the specified entity type.
        /// All repositories returned from the same UnitOfWork instance share the same DbContext.
        /// </summary>
        /// <typeparam name="TEntity">The entity type derived from BaseEntity</typeparam>
        /// <returns>Repository instance for the specified entity type</returns>
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity, new();

        /// <summary>
        /// Saves all changes made in this unit of work to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>The number of state entries written to the database</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Rolls back the current database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}

