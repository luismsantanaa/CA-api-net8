using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace AppApi.Authorization
{
    /// <summary>
    /// Attribute to mark endpoints that don't require authentication.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute
    {
    }

    /// <summary>
    /// Authorization attribute that delegates to the appropriate authorization strategy
    /// based on application configuration. Supports both standard ASP.NET Core JWT Bearer
    /// authentication and custom JWT middleware-based authentication.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Called when authorization is required.
        /// Determines the appropriate authorization strategy and delegates authorization to it.
        /// </summary>
        /// <param name="context">The authorization filter context</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Skip authorization if action is decorated with [AllowAnonymous] attribute
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            // Get configuration and create appropriate strategy
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var strategy = AuthorizationStrategyFactory.CreateStrategy(configuration);

            // Delegate authorization to the selected strategy
            strategy.Authorize(context);
        }
    }
}

