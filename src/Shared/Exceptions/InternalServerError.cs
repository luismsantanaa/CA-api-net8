namespace Shared.Exceptions
{
    public class InternalServerError : ApplicationException
    {
        public InternalServerError(string message)
            : base(message)
        {
        }

        public InternalServerError(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}