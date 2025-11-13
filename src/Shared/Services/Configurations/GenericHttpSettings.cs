using RestSharp;
using RestSharp.Authenticators;
using Shared.Services.Enums;

namespace Shared.Services.Configurations
{
    public class GenericHttpSettings
    {
        public required string BaseUrl { get; set; }
        public required string EndPoint { get; set; }
        public required Method Method { get; set; }
        public IAuthenticator? Authenticator { get; set; }
        public Dictionary<string, object>? Headers { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
        public Dictionary<string, object>? QueryParameters { get; set; }
        public string? Body { get; set; }
        public string? Resource { get; set; }
        public HttpBodyType BodyType { get; set; } = HttpBodyType.JSON;
    }
}
