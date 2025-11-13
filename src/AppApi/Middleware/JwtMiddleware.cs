using AppApi.Authorization;

namespace AppApi.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userId = jwtUtils.ValidateToken(token!);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                // Attach user ID to context on successful JWT validation
                // The userId is stored as a string (matches the "uid" claim value)
                context.Items["User"] = userId;
            }

            await _next(context);
        }
    }
}
