using Persistence.Constants;

namespace Persistence.Caching.Contracts
{
    public interface ICacheKeyService : IScopedService
    {
        string GetDetailsKey(string entity, object pkId);
        string GetKey(string entity, object pkId);
        string GetListKey(string entity);
        string GetSelectListKey(string entity);
    }
}
