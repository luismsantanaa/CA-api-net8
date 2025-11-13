using Domain.Base;
using Persistence.DbContexts;
using Persistence.Repositories.Base;

namespace Persistence.Repositories.Application
{
    public class ApplicationRepository<TEntity> : RepositoryBase<TEntity, ApplicationDbContext> where TEntity : BaseEntity, new()
    {
        public ApplicationRepository(ApplicationDbContext context) : base(context) { }
    }
}
