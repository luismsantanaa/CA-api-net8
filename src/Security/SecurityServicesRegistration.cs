using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Security.Repositories.Concrete;
using Security.Repositories.Contracts;

namespace Security
{
    public static class SecurityServicesRegistration
    {
        public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RrHhContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("RRHHConnection")));

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            return services;
        }
    }
}
