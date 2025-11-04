using System.Net;
using Microsoft.Extensions.Configuration;
using Security.Entities;
using Security.Entities.DTOs;
using Shared.Services.Configurations;
using Shared.Services.Contracts;

namespace Security.Services.Concrete
{
    internal class EmployeeService
    {
        private readonly string _userName;
        private readonly IConfiguration _configuration;
        private readonly IGenericHttpClient _httpClient;
        private readonly IJsonService _jsonService;

        public EmployeeService(string userName, IConfiguration configuration, IGenericHttpClient httpClient, IJsonService jsonService)
        {
            _userName = userName;
            _configuration = configuration;
            _httpClient = httpClient;
            _jsonService = jsonService;
        }

        public async Task<VwEmployee>? GetEmployee()
        {
            var settings = GetSettings();
            var result = await _httpClient.Request(settings, default);

            if (result.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException(result.Content);

            var resultData = _jsonService.JsonToObject<RHEmployeesResult<VwEmployee>>(result!.Content!);

            return resultData!.Data!.FirstOrDefault()!;
        }

        private GenericHttpSettings GetSettings()
        {
            return new GenericHttpSettings
            {
                BaseUrl = _configuration["RRHHSettings:BaseUrl"]!,
                EndPoint = _configuration["RRHHSettings:EmplyeeEnpoint"]!,
                Method = RestSharp.Method.Get,
                Headers = new Dictionary<string, object>
                    {
                        { "Accept", "*/*" },
                        { "Content-Type", "application/json" }
                    },
                Parameters = new Dictionary<string, object>
                {
                    { _configuration["RRHHSettings:EmplyeeEnpointParameter"]!,$"{_userName}"}
                }
            };
        }
    }
}
