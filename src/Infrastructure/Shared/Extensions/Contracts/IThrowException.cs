namespace Shared.Extensions.Contracts
{
    public interface IThrowException { }

    public class ThrowException : IThrowException
    {
        public static IThrowException Exception { get; } = new ThrowException();

        public ThrowException() { }
    }
}
