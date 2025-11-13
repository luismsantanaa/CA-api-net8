using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AppApi.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void ConfigureSwagger(this IApplicationBuilder app)
        {
            // Only enable Swagger in Development environment for security
            if (app.ApplicationServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mardom.Template.Api");
                    options.RoutePrefix = "swagger";
                    options.DisplayRequestDuration();
                });
            }
        }

        public static void ConfigureHealthChecks(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            // Basic health check endpoint
            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            exception = e.Value.Exception?.Message,
                            duration = e.Value.Duration.TotalMilliseconds
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });

            // Health Checks UI (only in Development)
            if (environment.IsDevelopment())
            {
                app.UseHealthChecksUI(options =>
                {
                    options.UIPath = "/health-ui";
                    options.ApiPath = "/health-api";
                });
            }
        }
    }
}
