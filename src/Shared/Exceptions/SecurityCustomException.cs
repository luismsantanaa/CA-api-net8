namespace Shared.Exceptions
{
    public class SecurityCustomException : ApplicationException
    {
        public SecurityCustomException(string message)
            : base(message)
        {
        }

        public SecurityCustomException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}