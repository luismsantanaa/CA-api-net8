using Application.DTOs;
using Application.Features.Utilities.UploadFiles.Queries.Vm;
using AutoMapper;
using Domain.Entities.Shared;
using MediatR;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Utilities.UploadFiles.Queries
{
    public class GetUploadedFileByIdQuery : IRequest<Result<UploadedFileVm>>
    { public string? PkId { get; set; } }

    public class GetUploadedFileByIdQueryHandler(
        IRepositoryFactory _repositoryFactory,
        IMapper _mapper) : IRequestHandler<GetUploadedFileByIdQuery, Result<UploadedFileVm>>
    {

        public async Task<Result<UploadedFileVm>> Handle(GetUploadedFileByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var repoFileUpload = _repositoryFactory.GetRepository<UploadedFile>();

                // Use StringToGuid extension method for safe Guid conversion
                var fileId = request.PkId!.StringToGuid();
                var result = await repoFileUpload.GetAsync(x => x.Id == fileId, cancellationToken: cancellationToken);
                ThrowException.Exception.IfObjectClassNull(result, request.PkId!);

                var data = _mapper.Map<UploadedFileVm>(result);

                return Result<UploadedFileVm>.Success(data, 1);
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex);
                throw new InternalServerError(message, ex);
            }
        }
    }
}
