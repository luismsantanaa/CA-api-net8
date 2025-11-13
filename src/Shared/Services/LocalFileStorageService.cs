using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Exceptions;
using Shared.Services.Contracts;

namespace Shared.Services
{
    /// <summary>
    /// Local file system implementation of IFileStorageService
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
        {
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string directory, string? fileName = null, CancellationToken cancellationToken = default)
        {
            try
            {
                // Ensure directory exists
                await EnsureDirectoryExistsAsync(directory);

                // Use custom filename or original
                var finalFileName = fileName ?? file.FileName;
                var filePath = Path.Combine(directory, finalFileName);

                _logger.LogInformation("Saving file {FileName} to {FilePath}", finalFileName, filePath);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                _logger.LogInformation("File {FileName} saved successfully", finalFileName);

                return filePath;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied when saving file {FileName} to {Directory}", file.FileName, directory);
                throw new InternalServerError($"No tiene permisos para guardar el archivo en {directory}", ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error when saving file {FileName}", file.FileName);
                throw new InternalServerError($"Error de entrada/salida al guardar el archivo: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when saving file {FileName}", file.FileName);
                throw new InternalServerError($"Error inesperado al guardar el archivo: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await FileExistsAsync(filePath, cancellationToken))
                {
                    _logger.LogWarning("Attempted to delete non-existent file: {FilePath}", filePath);
                    return false;
                }

                _logger.LogInformation("Deleting file {FilePath}", filePath);

                // Run file deletion in Task.Run to avoid blocking
                await Task.Run(() => File.Delete(filePath), cancellationToken);

                _logger.LogInformation("File {FilePath} deleted successfully", filePath);

                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied when deleting file {FilePath}", filePath);
                throw new InternalServerError($"No tiene permisos para eliminar el archivo {filePath}", ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error when deleting file {FilePath}", filePath);
                throw new InternalServerError($"Error de entrada/salida al eliminar el archivo: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when deleting file {FilePath}", filePath);
                throw new InternalServerError($"Error inesperado al eliminar el archivo: {ex.Message}", ex);
            }
        }

        public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(File.Exists(filePath));
        }

        public async Task<Stream> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await FileExistsAsync(filePath, cancellationToken))
                {
                    throw new NotFoundException($"El archivo {filePath} no existe");
                }

                _logger.LogInformation("Opening file stream for {FilePath}", filePath);

                return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied when opening file {FilePath}", filePath);
                throw new InternalServerError($"No tiene permisos para leer el archivo {filePath}", ex);
            }
            catch (Exception ex) when (ex is not NotFoundException)
            {
                _logger.LogError(ex, "Error opening file stream for {FilePath}", filePath);
                throw new InternalServerError($"Error al abrir el archivo: {ex.Message}", ex);
            }
        }

        public Task<bool> EnsureDirectoryExistsAsync(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    _logger.LogInformation("Creating directory {DirectoryPath}", directoryPath);
                    Directory.CreateDirectory(directoryPath);
                }

                return Task.FromResult(true);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied when creating directory {DirectoryPath}", directoryPath);
                throw new InternalServerError($"No tiene permisos para crear el directorio {directoryPath}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directory {DirectoryPath}", directoryPath);
                throw new InternalServerError($"Error al crear el directorio: {ex.Message}", ex);
            }
        }
    }
}

