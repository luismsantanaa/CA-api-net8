using System.Linq.Expressions;
using Domain.Base;
using Persistence.Specification.Contracts;

namespace Persistence.Repositories.Contracts
{
    public interface IGenericRepository { }
    public interface IGenericRepository<TEntity> : IGenericRepository where TEntity : BaseEntity, new()
    {
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<int> AddRangeAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
        IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> spec);
        Task<int> CountAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(object id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetAllWithSpec(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, string? includeString = null, bool disableTracking = true, CancellationToken cancellationToken = default);
        Task<TEntity>? GetByIdAsync(object id, CancellationToken cancellationToken = default);
        Task<TEntity>? GetFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<TEntity> GetOneWithSpec(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);
        Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
