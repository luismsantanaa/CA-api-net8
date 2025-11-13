namespace Shared.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string name, object key) : base($"La Entidad [{name}]: {key},  no fue encontrada!")
        {
        }
        public NotFoundException(string name) : base($"La Entidad [{name}] no fue encontrada!")
        {
        }
    }
}
