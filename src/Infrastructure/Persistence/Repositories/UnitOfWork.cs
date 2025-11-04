using Domain.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.DbContexts;
using Persistence.Repositories.Application;
using Persistence.Repositories.Contracts;

namespace Persistence.Repositories
{
    /// <summary>
    /// Unit of Work implementation that coordinates database operations across multiple repositories.
    /// Ensures all repository operations share the same DbContext and can be committed as a single transaction.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly Dictionary<Type, object> _repositories = new();

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">The database context to use for this unit of work</param>
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets a repository for the specified entity type.
        /// Repositories are cached per UnitOfWork instance to ensure they share the same DbContext.
        /// </summary>
        /// <typeparam name="TEntity">The entity type derived from BaseEntity</typeparam>
        /// <returns>Repository instance for the specified entity type</returns>
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity, new()
        {
            var type = typeof(TEntity);
            
            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new ApplicationRepository<TEntity>(_context);
                _repositories[type] = repositoryInstance;
            }

            return (IGenericRepository<TEntity>)_repositories[type];
        }

        /// <summary>
        /// Saves all changes made in this unit of work to the database.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>The number of state entries written to the database</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Begins a new database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Commits the current database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Rolls back the current database transaction.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Disposes the unit of work and releases resources.
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _repositories.Clear();
        }
    }
}

