using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Security.Entities.DTOs;
using Security.Services.Contracts;
using Shared.Services.Configurations;
using Shared.Services.Contracts;
using Shared.Services.Enums;

namespace Security.Services.Concrete
{
    public class MardomAuthService : IMardomAuthService
    {
        private readonly AuthMardomApiSettings _authMardomApiSettings;
        private readonly IGenericHttpClient _httpClient;
        private readonly IJsonService _jsonService;
        private readonly ILogger<MardomAuthService> _logger;

        public MardomAuthService(IOptions<AuthMardomApiSettings> authMardomApiSettings, IGenericHttpClient httpClient, IJsonService jsonService, ILogger<MardomAuthService> logger)
        {
            _authMardomApiSettings = authMardomApiSettings.Value;
            _httpClient = httpClient;
            _jsonService = jsonService;
            _logger = logger;
        }

        public async Task<bool> ChangePassword(ChangePassword change)
        {
            try
            {
                var changePassword = new ChangePassword
                {
                    UserId = change.UserId,
                    Oldpassword = change.Oldpassword,
                    NewPassword = change.NewPassword,
                    ConfirmPassword = change.ConfirmPassword
                };

                var setting = GetSettings(changePassword);

                var result = await _httpClient.Request(setting, default);

                if (result.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException(result.ErrorException!.Message);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<bool> ResetPassword(string email)
        {
            try
            {
                var setting = GetResetSettings(email);

                var result = await _httpClient.Request(setting, default);

                if (result.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException(result.ErrorException!.Message);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<AuthResponse> UserAuthentication(AuthRequest login)
        {
            var myLogin = new MyLogin
            {
                userName = login.Email,
                password = login.Password,
                AppId = _authMardomApiSettings.LocationTls
            };

            var setting = GetLoginSettings(myLogin);

            var result = await _httpClient.Request(setting, default);

            if (result.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(result.Content);

            var resultData = _jsonService.JsonToObject<ContentResult>(result!.Content!);

            return resultData.Value!;
        }

        private GenericHttpSettings GetSettings(ChangePassword changePass)
        {
            var json = _jsonService.ObjectToJson(changePass);
            return new GenericHttpSettings
            {
                BaseUrl = _authMardomApiSettings.BaseUrl!,
                EndPoint = _authMardomApiSettings.ChangePassword!,
                Method = RestSharp.Method.Put,
                Headers = new Dictionary<string, object>
                {
                    { "Accept", "*/*" },
                    { "Content-Type", "application/json" }
                },
                BodyType = HttpBodyType.JSON,
                Body = json
            };
        }
        private GenericHttpSettings GetResetSettings(string email)
        {
            return new GenericHttpSettings
            {
                BaseUrl = _authMardomApiSettings.BaseUrl!,
                EndPoint = _authMardomApiSettings.ResetPassword!,
                Method = RestSharp.Method.Get,
                Headers = new Dictionary<string, object>
                {
                    { "Accept", "*/*" },
                    { "Content-Type", "application/json" }
                },
                Parameters = new Dictionary<string, object>
                {
                    { "userName", email}
                }
            };
        }
        private GenericHttpSettings GetLoginSettings(MyLogin loginData)
        {
            var json = _jsonService.ObjectToJson(loginData);
            return new GenericHttpSettings
            {
                BaseUrl = _authMardomApiSettings.BaseUrl!,
                EndPoint = _authMardomApiSettings.Login!,
                Method = RestSharp.Method.Post,
                Headers = new Dictionary<string, object>
                {
                    { "Accept", "*/*" },
                    { "Content-Type", "application/json" }
                },
                BodyType = HttpBodyType.JSON,
                Body = json
            };
        }

        private class ContentResult
        {
            public string? ContentType { get; set; }
            public string? SerializerSettings { get; set; }
            public string? StatusCode { get; set; }
            public AuthResponse Value { get; set; } = new AuthResponse();

        }
        private class MyLogin
        {
            public string? userName { get; set; }
            public string? password { get; set; }
            public string? AppId { get; set; }
        }
    }
}
