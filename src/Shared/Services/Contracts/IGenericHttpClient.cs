using RestSharp;
using Shared.Services.Configurations;

namespace Shared.Services.Contracts
{
    public interface IGenericHttpClient
    {
        Task<RestResponse> Request(GenericHttpSettings settings, CancellationToken cancellationToken);
    }
}
