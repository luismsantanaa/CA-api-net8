using System.Linq.Expressions;
using Domain.Base;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.Contracts;
using Persistence.Specification;
using Persistence.Specification.Contracts;

namespace Persistence.Repositories.Base
{
    /// <summary>
    /// Base repository implementation providing generic CRUD operations and specification pattern support.
    /// All repository operations use AsNoTracking() by default for read operations to improve performance.
    /// </summary>
    /// <typeparam name="TEntity">The entity type derived from BaseEntity</typeparam>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    public class RepositoryBase<TEntity, TContext> : IGenericRepository<TEntity> where TEntity : BaseEntity, new()
       where TContext : DbContext
    {
        /// <summary>
        /// The database context for this repository
        /// </summary>
        protected readonly DbContext _context;
        
        /// <summary>
        /// The DbSet for the entity type
        /// </summary>
        private DbSet<TEntity> _dbSet;

        /// <summary>
        /// Initializes a new instance of the RepositoryBase
        /// </summary>
        /// <param name="context">The database context</param>
        public RepositoryBase(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        #region Methods Async   
        /// <summary>
        /// Gets all entities without tracking for read-only operations.
        /// Uses AsNoTracking() to improve performance by not loading entities into the change tracker.
        /// </summary>
        public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default) 
            => await _context.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null,
                                       Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                                       string? includeString = null,
                                       bool disableTracking = true,
                                       CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> query = _dbSet;

            if (disableTracking) query = query.AsNoTracking();

            if (predicate != null) query = query.Where(predicate);

            foreach (var includeProperty in includeString!.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
                return await orderBy(query).ToListAsync(cancellationToken);

            return await query.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the first entity matching the predicate without tracking for read-only operations.
        /// Uses AsNoTracking() to improve performance by not loading entities into the change tracker.
        /// Note: If you need to update the entity, consider using GetByIdAsync or disable tracking explicitly.
        /// </summary>
        public async Task<TEntity>? GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
            return result!;
        }

        /// <summary>
        /// Gets an entity by its ID.
        /// Uses FindAsync which is optimized for primary key lookups and will check the change tracker first.
        /// For read-only operations where you don't need tracking, consider using GetFirstAsync with a predicate.
        /// </summary>
        public virtual async Task<TEntity>? GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            return result!;
        }

        /// <summary>
        /// Checks if an entity with the given ID exists.
        /// Uses FindAsync which is optimized for primary key lookups.
        /// </summary>
        public async Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default)
        {
            var result = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            return result != null;
        }

        /// <summary>
        /// Checks if any entity matches the filter without loading data into memory.
        /// Uses AnyAsync() with AsNoTracking() for optimal performance.
        /// </summary>
        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default)
        {
            if (filter == null)
                return await _dbSet.AnyAsync(cancellationToken);
            
            return await _dbSet.AsNoTracking().AnyAsync(filter, cancellationToken);
        }

        /// <summary>
        /// Adds an entity to the context without saving changes.
        /// Use UnitOfWork.SaveChangesAsync() to persist changes to the database.
        /// </summary>
        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            // Note: SaveChanges is now handled by UnitOfWork
            return entity;
        }

        /// <summary>
        /// Adds a range of entities to the context without saving changes.
        /// Use UnitOfWork.SaveChangesAsync() to persist changes to the database.
        /// </summary>
        public async Task<int> AddRangeAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            // Note: SaveChanges is now handled by UnitOfWork
            // Returns the number of entities added, not saved records
            return entities.Count;
        }

        /// <summary>
        /// Updates an entity in the context without saving changes.
        /// Use UnitOfWork.SaveChangesAsync() to persist changes to the database.
        /// </summary>
        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            // Use Task.CompletedTask to maintain async signature for consistency
            await Task.CompletedTask;
            _dbSet.Attach(entity);
            _dbSet.Entry(entity).State = EntityState.Modified;
            // Note: SaveChanges is now handled by UnitOfWork
            return entity;
        }

        /// <summary>
        /// Marks an entity for deletion without saving changes.
        /// Use UnitOfWork.SaveChangesAsync() to persist changes to the database.
        /// </summary>
        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            // Use Task.CompletedTask to maintain async signature for consistency
            await Task.CompletedTask;
            _dbSet.Remove(entity);
            // Note: SaveChanges is now handled by UnitOfWork
        }
        #endregion

        #region Specification Pattern
        public async Task<IReadOnlyList<TEntity>> GetAllWithSpec(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            var result = await ApplySpecification(spec).ToListAsync(cancellationToken);
            return result!;
        }

        public async Task<TEntity> GetOneWithSpec(ISpecification<TEntity> spec, CancellationToken cancellationToken = default)
        {
            var result = await ApplySpecification(spec).FirstOrDefaultAsync(cancellationToken);
            return result!;
        }

        public async Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default) 
            => await ApplySpecification(spec).CountAsync(cancellationToken);

        /// <summary>
        /// Applies a specification to the entity set and returns the queryable result.
        /// Automatically applies AsNoTracking() for read-only operations to improve performance.
        /// </summary>
        /// <param name="spec">The specification to apply</param>
        /// <returns>IQueryable result with the specification applied</returns>
        public IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec)
        {
            // Apply AsNoTracking() by default for read-only queries to improve performance
            return SpecificationEvaluator<TEntity>.GetQuery(_dbSet.AsNoTracking().AsQueryable(), spec);
        }
        #endregion
    }
}
