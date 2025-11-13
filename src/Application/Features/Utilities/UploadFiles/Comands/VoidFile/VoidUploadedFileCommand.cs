using Application.DTOs;
using Domain.Entities.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Persistence.Repositories.Contracts;
using Serilog.Context;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;
using Shared.Services.Contracts;

namespace Application.Features.Utilities.UploadFiles.Comands.VoidFile
{
    public class VoidUploadedFileCommand : IRequest<Result<string>>
    {
        public string? Id { get; set; }
        public bool PhysicalDelete { get; set; } = false; // Option to delete physical file
    }

    public class VoidUploadedFileCommandHandler(
        IUnitOfWork _unitOfWork,
        IFileStorageService _fileStorageService,
        ICacheInvalidationService _cacheInvalidationService,
        ILogger<VoidUploadedFileCommandHandler> _logger) 
        : IRequestHandler<VoidUploadedFileCommand, Result<string>>
    {
        public async Task<Result<string>> Handle(VoidUploadedFileCommand request, CancellationToken cancellationToken)
        {
            using (LogContext.PushProperty("FileId", request.Id))
            using (LogContext.PushProperty("PhysicalDelete", request.PhysicalDelete))
            {
                try
                {
                    _logger.LogInformation("Starting file void operation. FileId: {FileId}, PhysicalDelete: {PhysicalDelete}",
                        request.Id, request.PhysicalDelete);

                    var repoUploadFile = _unitOfWork.Repository<UploadedFile>();

                    Guid fileId;
                    try
                    {
                        fileId = Guid.Parse(request.Id!);
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogWarning(ex, "Invalid GUID format. Id: {Id}", request.Id);
                        throw;
                    }

                    var existUploadFile = await repoUploadFile.GetFirstAsync(
                        x => x.Id == fileId && (bool)x.Active!, 
                        cancellationToken)!;

                    ThrowException.Exception.IfObjectClassNull(existUploadFile, fileId);

                    _logger.LogInformation("File found. Name: {FileName}, Path: {FilePath}",
                        existUploadFile!.Name, existUploadFile.Path);

                    // Mark as inactive (soft delete)
                    existUploadFile.Active = false;

                    await repoUploadFile.UpdateAsync(existUploadFile, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("File marked as inactive in database");

                    // Physical delete if requested
                    if (request.PhysicalDelete && !string.IsNullOrEmpty(existUploadFile.Path))
                    {
                        try
                        {
                            var deleted = await _fileStorageService.DeleteFileAsync(existUploadFile.Path, cancellationToken);
                            
                            if (deleted)
                            {
                                _logger.LogInformation("Physical file deleted successfully. Path: {FilePath}", existUploadFile.Path);
                            }
                            else
                            {
                                _logger.LogWarning("Physical file not found for deletion. Path: {FilePath}", existUploadFile.Path);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error deleting physical file. Path: {FilePath}. Database record updated.", existUploadFile.Path);
                            // Don't throw - database was updated successfully
                        }
                    }

                    // Invalidate cache
                    await _cacheInvalidationService.InvalidateEntityListCacheAsync<UploadedFile>(cancellationToken);

                    _logger.LogInformation("File void operation completed successfully");

                    var message = $"El registro [{request.Id}] ({existUploadFile.Name}), ha sido eliminado de la entidad *UploadedFile* correctamente!";
                    return Result<string>.Success(request.Id!, 1, message);
                }
                catch (FormatException)
                {
                    throw;
                }
                catch (NotFoundException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorDeleting);
                    _logger.LogError(ex, "Unexpected error during file void operation. Error: {ErrorMessage}", message);
                    throw new InternalServerError(message, ex);
                }
            }
        }
    }
}
