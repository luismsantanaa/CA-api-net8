using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Behaviours
{
    /// <summary>
    /// Pipeline behavior for catching unhandled exceptions in MediatR requests.
    /// Enriches logs with structured context for better error tracing.
    /// </summary>
    public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TRequest> _logger;

        public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                var requestName = typeof(TRequest).Name;
                
                // Enrich log context with exception details
                using (LogContext.PushProperty("RequestType", requestName))
                using (LogContext.PushProperty("ExceptionType", ex.GetType().Name))
                {
                    _logger.LogError(ex, 
                        "Unhandled exception in request handler. Request: {RequestName}, ExceptionType: {ExceptionType}, Error: {ErrorMessage}", 
                        requestName, ex.GetType().Name, ex.Message);
                }
                
                throw;
            }
        }
    }
}
