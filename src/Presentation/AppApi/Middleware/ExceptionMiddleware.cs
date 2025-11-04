using System.Net;
using System.Text;
using Application.DTOs;
using Serilog.Context;
using Shared.Exceptions;
using Shared.Services.Contracts;

namespace AppApi.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Catches all unhandled exceptions and returns appropriate HTTP responses.
    /// Provides detailed error information in Development and generic messages in Production.
    /// Enriches logs with structured context including correlation ID, request details, and exception information.
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;
        private readonly IJsonService _jsonService;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";

        /// <summary>
        /// Initializes a new instance of the ExceptionMiddleware
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="logger">Logger instance for exception logging</param>
        /// <param name="env">Host environment to determine if running in Development</param>
        /// <param name="jsonService">Service for JSON serialization</param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env, IJsonService jsonService)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _jsonService = jsonService;
        }

        /// <summary>
        /// Invokes the middleware to handle exceptions in the request pipeline
        /// </summary>
        /// <param name="context">The HTTP context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            // Get correlation ID if available
            var correlationId = context.Response.Headers[CorrelationIdHeaderName].FirstOrDefault() 
                ?? context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault() 
                ?? "Unknown";

            // Capture additional request context
            var requestPath = context.Request.Path.ToString();
            var requestMethod = context.Request.Method;
            var queryString = context.Request.QueryString.ToString();
            var userIdentity = context.User?.Identity?.Name ?? "Anonymous";
            var userClaims = context.User?.Claims?.Select(c => $"{c.Type}={c.Value}").ToList() ?? new List<string>();
            
            // Capture inner exception details
            var innerExceptionMessage = ex.InnerException?.Message;
            var innerExceptionType = ex.InnerException?.GetType().Name;

            // Enrich log context with comprehensive exception details
            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
            using (LogContext.PushProperty("RequestPath", requestPath))
            using (LogContext.PushProperty("RequestMethod", requestMethod))
            using (LogContext.PushProperty("QueryString", queryString))
            using (LogContext.PushProperty("UserIdentity", userIdentity))
            using (LogContext.PushProperty("StatusCode", context.Response.StatusCode))
            using (LogContext.PushProperty("InnerExceptionType", innerExceptionType))
            {
                // Build detailed error message for logging
                var errorDetails = new StringBuilder();
                errorDetails.AppendLine($"Exception Type: {ex.GetType().Name}");
                errorDetails.AppendLine($"Message: {ex.Message}");
                if (!string.IsNullOrWhiteSpace(innerExceptionMessage))
                {
                    errorDetails.AppendLine($"Inner Exception: {innerExceptionType} - {innerExceptionMessage}");
                }
                errorDetails.AppendLine($"Path: {requestMethod} {requestPath}");
                if (!string.IsNullOrWhiteSpace(queryString))
                {
                    errorDetails.AppendLine($"Query: {queryString}");
                }
                errorDetails.AppendLine($"User: {userIdentity}");

                _logger.LogError(ex,
                    "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {RequestPath}, Method: {RequestMethod}, Query: {QueryString}, User: {UserIdentity}, InnerException: {InnerExceptionType}",
                    correlationId,
                    requestPath,
                    requestMethod,
                    queryString,
                    userIdentity,
                    innerExceptionType);
            }

            var validationJson = string.Empty;
            var statusCode = HttpStatusCode.InternalServerError; // 500 if unexpected
            ValidationException? validationException = null;

            // Determine status code and handle specific exception types
            // Order matters: more specific types must come before their base types
            switch (ex)
            {
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case ValidationException ve:
                    validationException = ve;
                    statusCode = HttpStatusCode.BadRequest;
                    validationJson = _jsonService.ObjectToJson(ve.Errors);
                    break;
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case SecurityCustomException:
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    break;
                case ArgumentNullException:
                case ArgumentException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    break;
            }

            // Build response message
            string message;
            string? details = null;
            
            if (!string.IsNullOrEmpty(validationJson) && validationException != null)
            {
                // For validation errors, provide a cleaner message structure
                message = "Errores de validación en la solicitud. Por favor, revise los errores detallados.";
            }
            else
            {
                message = ex.Message;
                
                // In non-production, include additional context
                if (!_env.IsProduction())
                {
                    var detailsBuilder = new StringBuilder();
                    detailsBuilder.AppendLine($"Exception Type: {ex.GetType().Name}");
                    
                    if (!string.IsNullOrWhiteSpace(innerExceptionMessage))
                    {
                        detailsBuilder.AppendLine($"Inner Exception ({innerExceptionType}): {innerExceptionMessage}");
                    }
                    
                    if (ex.Data?.Count > 0)
                    {
                        detailsBuilder.AppendLine("Additional Data:");
                        foreach (var key in ex.Data.Keys)
                        {
                            detailsBuilder.AppendLine($"  {key}: {ex.Data[key]}");
                        }
                    }
                    
                    details = detailsBuilder.ToString();
                }
            }

            var responseModel = Result<string>.Fail(message, statusCode.ToString());

            // In non-production, include stack trace and additional details
            if (!_env.IsProduction())
            {
                var stackTrace = ex.StackTrace;
                if (!string.IsNullOrWhiteSpace(details))
                {
                    stackTrace = $"{details}\n\nStack Trace:\n{stackTrace}";
                }
                responseModel = Result<string>.Fail(message, $"Error {(int)statusCode} - {statusCode}", stackTrace);
            }

            var result = _jsonService.ObjectToJson(responseModel);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            await context.Response.WriteAsync(result);
        }
    }
}
