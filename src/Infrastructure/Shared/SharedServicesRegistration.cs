using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Extensions.Contracts;
using Shared.Services;
using Shared.Services.Contracts;

namespace Shared
{
    public static class SharedServicesRegistration
    {
        public static void AddSharedServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IThrowException, ThrowException>();
            services.AddTransient<ISmtpMailService, SmtpMailService>();
            services.AddTransient<IGenericHttpClient, GenericHttpClientService>();
            services.AddTransient<ILocalTimeService, LocalTimeService>();
            services.AddTransient<IJsonService, JsonService>();
        }
    }
}
