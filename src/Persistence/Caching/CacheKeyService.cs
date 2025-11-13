using Persistence.Caching.Contracts;

namespace Persistence.Caching
{
    public class CacheKeyService : ICacheKeyService
    {
        public string GetListKey(string entity) => $"{entity}:List";

        public string GetSelectListKey(string entity) => $"{entity}:SelectList";

        public string GetKey(string entity, object pkId) => $"{entity}:{pkId}";

        public string GetDetailsKey(string entity, object pkId) => $"{entity}Details:{pkId}";
    }
}
