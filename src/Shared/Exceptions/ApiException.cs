
namespace Shared.Exceptions
{
    [Serializable]
    public class ApiException : ApplicationException
    {
        public ApiException() : base(ErrorMessage.UnknownException)
        {
        }
        public ApiException(string message, int code) : base(message)
        {
        }
    }
}