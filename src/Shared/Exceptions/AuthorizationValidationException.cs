namespace Shared.Exceptions
{
    public class AuthorizationValidationException : ApplicationException
    {
        public AuthorizationValidationException(string message)
            : base(message)
        {
        }
    }
}