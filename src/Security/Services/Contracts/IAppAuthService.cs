using Security.Entities;
using Security.Entities.DTOs;

namespace Security.Services.Contracts
{
    public interface IAppAuthService : IAuthentication
    {
        Task<RegistrationResponse> Register(RegistrationRequest request);
        Task<AuthResponse> RefreshToken(TokenRequest request);
    }
}
