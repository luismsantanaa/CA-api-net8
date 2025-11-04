using Security.Entities.DTOs;

namespace Security.Services.Contracts
{
    public interface IAuthentication
    {
        Task<AuthResponse> UserAuthentication(AuthRequest login);
    }
}
