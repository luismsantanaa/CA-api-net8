using Application.DTOs;
using Domain.Entities.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Persistence.Repositories.Contracts;
using Serilog.Context;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;
using Shared.Services.Contracts;
using Shared.Services.Enums;

namespace Application.Features.Utilities.UploadFiles.Comands.Create
{
    public class UploadFileCommand : IRequest<Result<bool>>
    {
        public List<IFormFile>? Files { get; set; }
        public string? Type { get; set; }
        public string? Reference { get; set; }
        public string? Comment { get; set; }
    }

    public class UploadFileCommandHandler(
        IUnitOfWork _unitOfWork,
        IConfiguration _configuration,
        IFileStorageService _fileStorageService,
        ICacheInvalidationService _cacheInvalidationService,
        ILogger<UploadFileCommandHandler> _logger) 
        : IRequestHandler<UploadFileCommand, Result<bool>>
    {
        private readonly long _maxFileSize = 3000000; // 3MB - should be from configuration

        public async Task<Result<bool>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var uploadedFiles = new List<UploadedFile>();
            var savedFiles = new List<string>();

            using (LogContext.PushProperty("FileCount", request.Files?.Count ?? 0))
            using (LogContext.PushProperty("FileType", request.Type))
            using (LogContext.PushProperty("Reference", request.Reference))
            {
                try
                {
                    _logger.LogInformation("Starting file upload. Files: {FileCount}, Type: {Type}, Reference: {Reference}",
                        request.Files?.Count ?? 0, request.Type, request.Reference);

                    // Validations
                    ThrowException.Exception.IfNull(request.Files, "Files list cannot be null");
                    
                    if (request.Files!.Count == 0)
                    {
                        throw new BadRequestException("At least one file must be provided");
                    }

                    var path = _configuration["FilesPaths:TestPath"];
                    
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new BadRequestException("FilesPaths:TestPath configuration is missing");
                    }

                    // Ensure directory exists
                    await _fileStorageService.EnsureDirectoryExistsAsync(path!);

                    var fileRepository = _unitOfWork.Repository<UploadedFile>();
                    var extensions = FileValidExtensions.ValidFiles;

                    // Validate all files first (fail fast)
                    _logger.LogInformation("Validating {Count} files", request.Files.Count);
                    foreach (var file in request.Files)
                    {
                        var extFile = Path.GetExtension(file.FileName);
                        
                        if (!extensions.Contains(extFile))
                        {
                            var message = $"El Archivo [{file.FileName}] no tiene una extensión de archivo permitida para almacenar!!";
                            _logger.LogWarning(message);
                            throw new BadRequestException(message);
                        }

                        if (file.Length > _maxFileSize)
                        {
                            var message = $"El Archivo [{file.FileName}] excede los {_maxFileSize}MB permitidos para Almacenar!";
                            _logger.LogWarning(message);
                            throw new BadRequestException(message);
                        }
                    }

                    _logger.LogInformation("All files validated successfully. Starting file save process");

                    // Start transaction
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);

                    try
                    {
                        // Process each file
                        foreach (var formFile in request.Files)
                        {
                            _logger.LogInformation("Processing file {FileName} ({Size} bytes)", formFile.FileName, formFile.Length);

                            // Save file to storage
                            var filePath = await _fileStorageService.SaveFileAsync(
                                formFile,
                                path!,
                                cancellationToken: cancellationToken);

                            savedFiles.Add(filePath);

                            // Create database record
                            var uploadedFile = new UploadedFile
                            {
                                Name = formFile.FileName,
                                Type = request.Type,
                                Reference = request.Reference,
                                Size = Convert.ToDecimal(ConvertBytesToMegabytes(formFile.Length)),
                                Comment = request.Comment,
                                Extension = Path.GetExtension(formFile.FileName),
                                Path = filePath
                            };

                            await fileRepository.AddAsync(uploadedFile, cancellationToken);
                            uploadedFiles.Add(uploadedFile);

                            _logger.LogInformation("File {FileName} processed successfully. Path: {Path}", 
                                formFile.FileName, filePath);
                        }

                        // Save to database
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        // Commit transaction
                        await _unitOfWork.CommitTransactionAsync(cancellationToken);

                        _logger.LogInformation("All files saved successfully. Total: {Count}", uploadedFiles.Count);

                        // Invalidate cache
                        await _cacheInvalidationService.InvalidateEntityListCacheAsync<UploadedFile>(cancellationToken);

                        return Result<bool>.Success(true, uploadedFiles.Count, 
                            $"{uploadedFiles.Count} file(s) uploaded successfully");
                    }
                    catch (Exception)
                    {
                        // Rollback transaction
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                        // Cleanup saved files
                        _logger.LogWarning("Error occurred. Rolling back and cleaning up {Count} saved files", savedFiles.Count);
                        await CleanupSavedFiles(savedFiles, cancellationToken);

                        throw;
                    }
                }
                catch (BadRequestException ex)
                {
                    _logger.LogWarning(ex, "Validation error during file upload");
                    throw;
                }
                catch (Exception ex)
                {
                    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
                    _logger.LogError(ex, "Unexpected error during file upload. Error: {ErrorMessage}", message);
                    throw new InternalServerError(message, ex);
                }
            }
        }

        private async Task CleanupSavedFiles(List<string> filePaths, CancellationToken cancellationToken)
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(filePath, cancellationToken);
                    _logger.LogInformation("Cleaned up file: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to cleanup file: {FilePath}", filePath);
                    // Continue cleanup even if one fails
                }
            }
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return bytes / 1024f / 1024f;
        }
    }
}
