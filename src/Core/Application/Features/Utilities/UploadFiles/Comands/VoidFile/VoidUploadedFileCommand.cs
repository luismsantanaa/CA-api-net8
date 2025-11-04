using Application.DTOs;
using Domain.Entities.Shared;
using MediatR;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Utilities.UploadFiles.Comands.VoidFile
{
    public class VoidUploadedFileCommand : IRequest<Result<string>>
    {
        public string? Id { get; set; }
    }

    public class VoidUploadedFileCommandHandler(
        IUnitOfWork _unitOfWork) : IRequestHandler<VoidUploadedFileCommand, Result<string>>
    {

        public async Task<Result<string>> Handle(VoidUploadedFileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var repoUploadFile = _unitOfWork.Repository<UploadedFile>();

                var existUploadFile = await repoUploadFile.GetFirstAsync(x => x.Id == Guid.Parse(request.Id!) && (bool)x.Active!, cancellationToken)!;
                ThrowException.Exception.IfObjectClassNull(existUploadFile, request.Id!);

                existUploadFile.Active = false;

                await repoUploadFile.UpdateAsync(existUploadFile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result<string>.Success(request.Id!, 1, ErrorMessage.DeletedSuccessfully("UploadedFiles", existUploadFile!.Name!));
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorDeleting);
                throw new InternalServerError(message, ex);
            }
        }
    }
}
