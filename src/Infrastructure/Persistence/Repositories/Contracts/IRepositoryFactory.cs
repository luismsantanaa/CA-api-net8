using Domain.Base;

namespace Persistence.Repositories.Contracts
{
    public interface IRepositoryFactory
    {
        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new();
        TRepository GetCustomRepository<TRepository>() where TRepository : IGenericRepository;
    }
}
