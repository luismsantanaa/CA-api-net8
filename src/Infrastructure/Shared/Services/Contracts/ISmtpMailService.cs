using Shared.Services.Configurations;

namespace Shared.Services.Contracts
{
    public interface ISmtpMailService
    {
        Task<bool> SendAsync(MailRequest request, string? pathImages);
    }
}
