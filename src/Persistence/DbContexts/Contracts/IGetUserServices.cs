namespace Persistence.DbContexts.Contracts
{
    public interface IGetUserServices
    {
        bool? IsAuthenticated { get; }
        Guid? UserId { get; }
    }
}
