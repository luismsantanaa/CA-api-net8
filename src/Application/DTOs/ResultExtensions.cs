namespace Application.DTOs
{
    /// <summary>
    /// Extension methods for Result<T> to provide helper methods for common operations.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Checks if the result is successful.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="result">The result to check</param>
        /// <returns>True if the result is successful, false otherwise</returns>
        public static bool IsSuccess<T>(this Result<T> result)
        {
            return result?.Succeeded == true;
        }

        /// <summary>
        /// Checks if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="result">The result to check</param>
        /// <returns>True if the result is a failure, false otherwise</returns>
        public static bool IsFailure<T>(this Result<T> result)
        {
            return result?.Succeeded == false;
        }

        /// <summary>
        /// Gets the value if the result is successful, otherwise returns the default value.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="result">The result</param>
        /// <param name="defaultValue">The default value to return if the result is a failure</param>
        /// <returns>The result value if successful, otherwise the default value</returns>
        public static T? GetValueOrDefault<T>(this Result<T> result, T? defaultValue = default)
        {
            return result?.IsSuccess() == true ? result.Items : defaultValue;
        }

        /// <summary>
        /// Transforms the result to a new type if successful, otherwise preserves the failure.
        /// </summary>
        /// <typeparam name="TSource">The source type</typeparam>
        /// <typeparam name="TTarget">The target type</typeparam>
        /// <param name="result">The source result</param>
        /// <param name="transform">The transformation function to apply if successful</param>
        /// <returns>A new Result with the transformed value if successful, otherwise a failure result</returns>
        public static Result<TTarget> Map<TSource, TTarget>(this Result<TSource> result, Func<TSource?, TTarget> transform)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.IsFailure())
            {
                return Result<TTarget>.Fail(
                    result.FriendlyMessage ?? "Operation failed",
                    result.StatusCode ?? "500",
                    result.StackTrace);
            }

            var transformedValue = transform(result.Items);
            return Result<TTarget>.Success(transformedValue, result.Total, result.FriendlyMessage);
        }

        /// <summary>
        /// Creates a successful result from a single item with total count of 1.
        /// Useful for single-item queries.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="item">The item to wrap in a success result</param>
        /// <param name="message">Optional friendly message</param>
        /// <returns>A successful Result containing the item</returns>
        public static Result<T> ToResult<T>(this T item, string? message = null)
        {
            return Result<T>.Success(item, 1, message);
        }

        /// <summary>
        /// Creates a successful result from a collection with the count as total.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="items">The collection to wrap in a success result</param>
        /// <param name="message">Optional friendly message</param>
        /// <returns>A successful Result containing the collection</returns>
        public static Result<IReadOnlyList<T>> ToResultList<T>(this IEnumerable<T> items, string? message = null)
        {
            var itemList = items.ToList();
            return Result<IReadOnlyList<T>>.Success(itemList, itemList.Count, message);
        }

        /// <summary>
        /// Executes an action if the result is successful.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="result">The result</param>
        /// <param name="onSuccess">The action to execute if successful</param>
        /// <returns>The original result</returns>
        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T?> onSuccess)
        {
            if (result?.IsSuccess() == true)
            {
                onSuccess(result.Items);
            }
            return result!;
        }

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="result">The result</param>
        /// <param name="onFailure">The action to execute if failed</param>
        /// <returns>The original result</returns>
        public static Result<T> OnFailure<T>(this Result<T> result, Action<string?> onFailure)
        {
            if (result?.IsFailure() == true)
            {
                onFailure(result.FriendlyMessage);
            }
            return result!;
        }

        /// <summary>
        /// Creates a failure result with a standard HTTP status code.
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="message">The error message</param>
        /// <param name="statusCode">The HTTP status code (default: 500)</param>
        /// <returns>A failure Result</returns>
        public static Result<T> Failure<T>(string message, string statusCode = "500")
        {
            return Result<T>.Fail(message, statusCode);
        }

        /// <summary>
        /// Creates a success result without a message (common pattern).
        /// </summary>
        /// <typeparam name="T">The type of the result data</typeparam>
        /// <param name="data">The data to wrap</param>
        /// <param name="total">The total count (default: 1)</param>
        /// <returns>A successful Result</returns>
        public static Result<T> SuccessResult<T>(T data, int total = 1)
        {
            return Result<T>.Success(data, total);
        }

        /// <summary>
        /// Creates a successful result for a created entity operation.
        /// Commonly used after Create operations.
        /// </summary>
        /// <param name="entityId">The ID of the created entity</param>
        /// <param name="entityName">The name/type of the entity (e.g., "Product", "Category")</param>
        /// <param name="entityIdentifier">Optional identifier for the entity (e.g., name, code)</param>
        /// <returns>A successful Result with the entity ID as string</returns>
        public static Result<string> CreatedSuccessfully(Guid entityId, string entityName, string? entityIdentifier = null)
        {
            var message = Shared.Exceptions.ErrorMessage.AddedSuccessfully(entityName, entityIdentifier ?? entityId.ToString());
            return Result<string>.Success(entityId.ToString(), 1, message);
        }

        /// <summary>
        /// Creates a successful result for an updated entity operation.
        /// Commonly used after Update operations.
        /// </summary>
        /// <param name="entityId">The ID of the updated entity</param>
        /// <param name="entityName">The name/type of the entity</param>
        /// <param name="entityIdentifier">Optional identifier for the entity</param>
        /// <returns>A successful Result with the entity ID as string</returns>
        public static Result<string> UpdatedSuccessfully(string entityId, string entityName, string? entityIdentifier = null)
        {
            var message = Shared.Exceptions.ErrorMessage.UpdatedSuccessfully(entityName, entityIdentifier ?? entityId);
            return Result<string>.Success(entityId, 1, message);
        }

        /// <summary>
        /// Creates a successful result for a deleted entity operation.
        /// Commonly used after Delete operations.
        /// </summary>
        /// <param name="entityId">The ID of the deleted entity</param>
        /// <param name="entityName">The name/type of the entity</param>
        /// <returns>A successful Result with the entity ID as string</returns>
        public static Result<string> DeletedSuccessfully(string entityId, string entityName)
        {
            var message = Shared.Exceptions.ErrorMessage.DeletedSuccessfully(entityName, entityId);
            return Result<string>.Success(entityId, 1, message);
        }
    }
}

