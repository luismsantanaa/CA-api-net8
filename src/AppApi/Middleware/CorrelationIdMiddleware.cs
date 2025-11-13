using Serilog.Context;

namespace AppApi.Middleware
{
    /// <summary>
    /// Middleware to add correlation ID to each request for tracing purposes.
    /// Adds correlation ID to LogContext for structured logging and to response headers.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get or create correlation ID from request header or generate new one
            var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            // Add correlation ID to response headers
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;

            // Add correlation ID to LogContext for all logs in this request scope
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            {
                await _next(context);
            }
        }
    }
}

