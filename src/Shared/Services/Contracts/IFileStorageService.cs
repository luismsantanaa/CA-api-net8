using Microsoft.AspNetCore.Http;

namespace Shared.Services.Contracts
{
    /// <summary>
    /// Service for handling file storage operations.
    /// Abstracts file system operations to allow for different storage implementations (local, Azure Blob, S3, etc.)
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves a file to the specified directory
        /// </summary>
        /// <param name="file">The file to save</param>
        /// <param name="directory">The directory where the file will be saved</param>
        /// <param name="fileName">Optional custom filename. If null, uses original filename</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The full path where the file was saved</returns>
        Task<string> SaveFileAsync(IFormFile file, string directory, string? fileName = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if file was deleted, false if file didn't exist</returns>
        Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file exists
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if file exists</returns>
        Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a stream to read a file
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Stream to read the file</returns>
        Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ensures a directory exists, creating it if necessary
        /// </summary>
        /// <param name="directoryPath">Path to the directory</param>
        /// <returns>True if directory exists or was created</returns>
        Task<bool> EnsureDirectoryExistsAsync(string directoryPath);
    }
}

