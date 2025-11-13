using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using System;

namespace Application.Behaviours
{
    /// <summary>
    /// Pipeline behavior that validates requests using FluentValidation before handling them.
    /// Provides comprehensive error handling and logging for validation failures.
    /// </summary>
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehaviour<TRequest, TResponse>>? _logger;

        public ValidationBehaviour(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehaviour<TRequest, TResponse>>? logger = null)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var context = new ValidationContext<TRequest>(request);
            var requestType = typeof(TRequest).Name;

            try
            {
                var validationResults = await Task.WhenAll(
                    _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .SelectMany(r => r.Errors)
                    .Where(f => f != null)
                    .ToList();

                if (failures.Count != 0)
                {
                    _logger?.LogWarning(
                        "Validation failed for {RequestType}. Failures: {FailureCount}. Errors: {Errors}",
                        requestType,
                        failures.Count,
                        string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}")));

                    throw new Shared.Exceptions.ValidationException(failures);
                }

                _logger?.LogDebug("Validation passed for {RequestType}", requestType);
            }
            catch (Shared.Exceptions.ValidationException)
            {
                // Re-throw validation exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                // Handle exceptions that occur during validation (e.g., database errors in async validators)
                _logger?.LogError(ex,
                    "An error occurred during validation for {RequestType}. Error: {ErrorMessage}",
                    requestType,
                    ex.Message);

                // Wrap non-validation exceptions in a ValidationException to maintain consistent error handling
                var validationFailures = new List<ValidationFailure>
                {
                    new ValidationFailure(
                        "ValidationError",
                        $"An error occurred during validation: {ex.Message}")
                };

                throw new Shared.Exceptions.ValidationException(validationFailures);
            }

            return await next();
        }
    }
}
