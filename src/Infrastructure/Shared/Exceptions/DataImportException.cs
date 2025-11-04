namespace Shared.Exceptions
{
    public class DataImportException : ApplicationException
    {
        public DataImportException(string message)
            : base(message)
        {
        }

        public DataImportException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}