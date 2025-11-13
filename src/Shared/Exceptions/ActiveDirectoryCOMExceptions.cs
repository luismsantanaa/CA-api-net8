
namespace Shared.Exceptions
{
    public class ActiveDirectoryCOMExceptions : ApplicationException
    {
        public ActiveDirectoryCOMExceptions(Exception inner)
            : base(ErrorMessage.ActiveDirectoryCOMExceptions, inner)
        {
        }
    }
}
