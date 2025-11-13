using Security.Entities;

namespace Security.Services.Contracts
{
    public interface IActiveDirectoryService
    {
        Task<bool> Authenticate(string email, string password);
        Task<UserAzureAD> GetUser(string username);
        Task<bool> UserExist(string usermail);
    }
}
