using Security.Entities.DTOs;

namespace Security.Services.Contracts
{
    public interface IMardomAuthService : IAuthentication
    {
        Task<bool> ChangePassword(ChangePassword change);
        Task<bool> ResetPassword(string email);
    }
}
