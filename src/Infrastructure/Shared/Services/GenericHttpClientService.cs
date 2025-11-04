using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestSharp;
using Shared.Services.Configurations;
using Shared.Services.Contracts;
using Shared.Services.Enums;

namespace Shared.Services
{
    public class GenericHttpClientService : IGenericHttpClient
    {
        private readonly ILogger<GenericHttpClientService> _logger;

        public GenericHttpClientService(ILogger<GenericHttpClientService> logger)
        {
            _logger = logger;
        }

        public async Task<RestResponse> Request(GenericHttpSettings settings, CancellationToken cancellationToken)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

            var request = new RestRequest(settings.EndPoint, settings.Method);
            var options = new RestClientOptions(settings.BaseUrl)
            {
                Authenticator = settings.Authenticator,
            };
            var client = new RestClient(options);

            if (settings.Headers != null && settings.Headers.Count > 0)
                foreach (var key in settings.Headers.Keys)
                {
                    if (settings.Headers[key].GetType().ToString().StartsWith("System.Collections.Generics.List"))
                    {
                        request.AddHeader(key, JsonSerializer.Serialize(settings.Headers[key]));
                    }
                    else
                    {
                        request.AddHeader(key, settings!.Headers[key]!.ToString()!);
                    }
                }

            if (settings.Parameters != null && settings.Parameters.Count > 0)
                foreach (var key in settings.Parameters.Keys)
                {
                    request.AddParameter(key, settings.Parameters[key], ParameterType.GetOrPost);
                }

            if (settings.QueryParameters != null && settings.QueryParameters.Count > 0)
                foreach (var key in settings.QueryParameters.Keys)
                {
                    if (settings!.Headers![key].GetType().ToString().StartsWith("System.Collections.Generics.List"))
                    {
                        request.AddQueryParameter(key, JsonSerializer.Serialize(settings.QueryParameters[key]));
                    }
                    else
                    {
                        request.AddQueryParameter(key, settings.QueryParameters[key].ToString());
                    }
                }

            if (!string.IsNullOrEmpty(settings.Body))
            {
                switch (settings.BodyType)
                {
                    case HttpBodyType.JSON:
                        var document = JsonDocument.Parse(settings.Body);
                        request.AddBody(document.RootElement);
                        break;

                    case HttpBodyType.TEXT:
                        request.AddBody(settings.Body);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(settings.Resource))
            {
                request.Resource += settings.Resource;
            }

            RestResponse? response;

            try
            {
                response = await client.ExecuteAsync(request, cancellationToken);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
