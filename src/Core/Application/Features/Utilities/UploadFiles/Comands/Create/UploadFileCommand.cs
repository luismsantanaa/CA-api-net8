using Application.DTOs;
using Domain.Entities.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;
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
        IConfiguration _configuration) : IRequestHandler<UploadFileCommand, Result<bool>>
    {

        public async Task<Result<bool>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var path = _configuration["FilesPaths:TestPath"];
            try
            {
                var fileRepository = _unitOfWork.Repository<UploadedFile>();

                var extensions = FileValidExtensions.ValidFiles;

                foreach (var item in request.Files!)
                {
                    var extFile = Path.GetExtension(item.FileName);
                    var maxLen = (long)3000000.00;

                    ThrowException.Exception.IfFileHasBadExtension(extensions, extFile, item.FileName);
                    ThrowException.Exception.IfFileToLarge(maxLen, item.Length, item.FileName);
                }

                foreach (var formFile in request.Files)
                {
                    var file = new UploadedFile();
                    file.Name = formFile.FileName;
                    file.Type = request.Type;
                    file.Reference = request.Reference;
                    file.Size = Convert.ToDecimal(ConvertBytesToMegabytes(formFile.Length));
                    file.Comment = request.Comment;
                    file.Extension = Path.GetExtension(formFile.FileName);
                    file.Path = Path.Combine(path!, formFile.FileName);

                    using (var stream = File.Create(file.Path))
                    {
                        await formFile.CopyToAsync(stream, cancellationToken);
                    }

                    await fileRepository.AddAsync(file, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<bool>.Success(true, 1);
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
                throw new InternalServerError(message, ex);
            }
        }

        private static double ConvertBytesToMegabytes(long bytes)
        {
            return bytes / 1024f / 1024f;
        }

    }
}
