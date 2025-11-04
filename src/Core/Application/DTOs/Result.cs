using Application.Contracts;

namespace Application.DTOs
{
    /// <summary>
    /// Represents the result of an operation that can either succeed or fail.
    /// Provides a consistent way to return operation results with success/failure status,
    /// data, error messages, and metadata.
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
    public class Result<T> : IResult<T>
    {
        /// <summary>
        /// Indicates whether the operation succeeded
        /// </summary>
        public bool Succeeded { get; set; }
        
        /// <summary>
        /// User-friendly message describing the result
        /// </summary>
        public string? FriendlyMessage { get; set; }
        
        /// <summary>
        /// The data returned on success, or null on failure
        /// </summary>
        public T? Items { get; set; }
        
        /// <summary>
        /// Total count of items (useful for pagination or lists)
        /// </summary>
        public int Total { get; set; }
        
        /// <summary>
        /// HTTP status code associated with the result
        /// </summary>
        public string? StatusCode { get; set; }
        
        /// <summary>
        /// Stack trace (only in non-production environments)
        /// </summary>
        public string? StackTrace { get; set; }

        /// <summary>
        /// Creates a failed result with an error message and status code.
        /// </summary>
        /// <param name="friendlyMessage">User-friendly error message</param>
        /// <param name="statusCode">HTTP status code as string (e.g., "400", "500")</param>
        /// <param name="stackTrace">Optional stack trace for debugging</param>
        /// <returns>A failed Result instance</returns>
        public static Result<T> Fail(string friendlyMessage, string statusCode, string? stackTrace = null)
        {
            return new Result<T>
            {
                Succeeded = false,
                FriendlyMessage = friendlyMessage,
                StackTrace = stackTrace,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Creates a successful result with data and optional message.
        /// </summary>
        /// <param name="data">The data to return</param>
        /// <param name="total">Total count of items (default: 1 for single items, or count for collections)</param>
        /// <param name="message">Optional user-friendly success message</param>
        /// <returns>A successful Result instance</returns>
        public static Result<T> Success(T data, int total, string? message = null)
        {
            return new Result<T> { Succeeded = true, Items = data, Total = total, FriendlyMessage = message };
        }
    }
}
