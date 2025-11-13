using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Diagnostics;

namespace Application.Behaviours
{
    /// <summary>
    /// Pipeline behavior for logging MediatR requests and responses.
    /// Enriches logs with structured context including execution time and request details.
    /// </summary>
    public sealed class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

        public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            // Enrich log context with request information
            using (LogContext.PushProperty("RequestType", requestName))
            using (LogContext.PushProperty("RequestName", requestName))
            {
                // Log request start
                _logger.LogInformation(
                    "Handling {RequestName} at {UtcNow}",
                    requestName,
                    DateTime.UtcNow);

                try
                {
                    var result = await next();
                    stopwatch.Stop();

                    // Log successful completion with execution time
                    _logger.LogInformation(
                        "Request {RequestName} completed successfully in {ElapsedMilliseconds}ms",
                        requestName,
                        stopwatch.ElapsedMilliseconds);

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    
                    // Log error with execution time and exception details
                    _logger.LogError(ex,
                        "Request {RequestName} failed after {ElapsedMilliseconds}ms. Error: {ErrorMessage}",
                        requestName,
                        stopwatch.ElapsedMilliseconds,
                        ex.Message);

                    throw;
                }
            }
        }
    }
}
