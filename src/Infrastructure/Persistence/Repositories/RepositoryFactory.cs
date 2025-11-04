using Domain.Base;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Repositories.Contracts;

namespace Persistence.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider? _service;
        public RepositoryFactory() { }
        public RepositoryFactory(IServiceProvider service)
        {
            _service = service;
        }

        public TRepository GetCustomRepository<TRepository>() where TRepository : IGenericRepository
        {
            var instance = _service!.GetService<TRepository>();
            return instance!;
        }
        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity, new()
        {
            var instance = _service!.GetService<IGenericRepository<TEntity>>();
            return instance!;
        }
    }
}
