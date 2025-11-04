using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppApi.Authorization
{
    /// <summary>
    /// Interface for authorization strategies.
    /// Allows different authorization mechanisms to be implemented independently.
    /// </summary>
    public interface IAuthorizationStrategy
    {
        /// <summary>
        /// Determines whether the current request is authorized.
        /// </summary>
        /// <param name="context">The authorization filter context</param>
        /// <returns>True if authorized, false otherwise. Sets context.Result if unauthorized.</returns>
        bool Authorize(AuthorizationFilterContext context);
    }

    /// <summary>
    /// Standard ASP.NET Core authorization strategy.
    /// Uses the built-in authentication middleware (JWT Bearer) to validate tokens.
    /// </summary>
    public class StandardAuthorizationStrategy : IAuthorizationStrategy
    {
        public bool Authorize(AuthorizationFilterContext context)
        {
            var authenticatedUser = context.HttpContext?.User;
            var isAuthenticated = authenticatedUser?.Identity?.IsAuthenticated == true;

            if (!isAuthenticated)
            {
                context.Result = new JsonResult(new
                {
                    Succeeded = false,
                    FriendlyMessage = "Unauthorized. Please provide a valid authentication token.",
                    StatusCode = StatusCodes.Status401Unauthorized.ToString(),
                })
                { StatusCode = StatusCodes.Status401Unauthorized };
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Custom authorization strategy using JwtMiddleware.
    /// Validates tokens using custom JWT validation logic and stores user info in HttpContext.Items.
    /// </summary>
    public class CustomAuthorizationStrategy : IAuthorizationStrategy
    {
        public bool Authorize(AuthorizationFilterContext context)
        {
            // Custom authorization logic using Items["User"] (requires JwtMiddleware)
            string? user;
            try
            {
                user = context.HttpContext?.Items["User"]?.ToString();
            }
            catch
            {
                user = null;
            }

            if (user == null)
            {
                context.Result = new JsonResult(new
                {
                    Succeeded = false,
                    FriendlyMessage = "Unauthorized",
                    StatusCode = StatusCodes.Status401Unauthorized.ToString(),
                })
                { StatusCode = StatusCodes.Status401Unauthorized };
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Factory for creating authorization strategies based on configuration.
    /// </summary>
    public static class AuthorizationStrategyFactory
    {
        /// <summary>
        /// Creates an appropriate authorization strategy based on application configuration.
        /// </summary>
        /// <param name="configuration">Application configuration to read settings from</param>
        /// <returns>An IAuthorizationStrategy instance</returns>
        public static IAuthorizationStrategy CreateStrategy(IConfiguration configuration)
        {
            var useCustomAuthorization = configuration.GetValue<bool>("AuthorizationSettings:UseCustomAuthorization", true);

            return useCustomAuthorization
                ? new CustomAuthorizationStrategy()
                : new StandardAuthorizationStrategy();
        }
    }
}

