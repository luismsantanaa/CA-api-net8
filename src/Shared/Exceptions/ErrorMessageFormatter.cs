namespace Shared.Exceptions
{
    /// <summary>
    /// Helper class for formatting error messages consistently across the application.
    /// Centralizes the logic for combining base error messages with exception details.
    /// </summary>
    public static class ErrorMessageFormatter
    {
        /// <summary>
        /// Formats an error message by combining a base message with exception details.
        /// Prioritizes InnerException message if available, otherwise uses the main exception message.
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <param name="baseMessage">The base error message to prepend</param>
        /// <returns>Formatted error message in the format: "{baseMessage}: {exception message}"</returns>
        public static string Format(Exception ex, string baseMessage)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            if (string.IsNullOrWhiteSpace(baseMessage))
                throw new ArgumentException("Base message cannot be null or empty", nameof(baseMessage));

            var exceptionMessage = ex.InnerException?.Message ?? ex.Message;
            return $"{baseMessage}: {exceptionMessage}";
        }

        /// <summary>
        /// Formats an error message without a base message prefix.
        /// Used for cases where only the exception message is needed.
        /// </summary>
        /// <param name="ex">The exception that occurred</param>
        /// <returns>The exception message (from InnerException if available, otherwise from the main exception)</returns>
        public static string Format(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            return ex.InnerException?.Message ?? ex.Message;
        }
    }
}

