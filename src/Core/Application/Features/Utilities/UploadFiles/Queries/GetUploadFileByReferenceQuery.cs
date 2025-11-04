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
    public class GetUploadFileByReferenceQuery : IRequest<Result<UploadedFileVm>>
    {
        public string? Reference { get; set; }
    }

    public class GetUploadedFileByReferenceQueryHandler(
        IRepositoryFactory _repositoryFactory,
        IMapper _mapper) : IRequestHandler<GetUploadFileByReferenceQuery, Result<UploadedFileVm>>
    {

        public async Task<Result<UploadedFileVm>> Handle(GetUploadFileByReferenceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var repoFileUpload = _repositoryFactory.GetRepository<UploadedFile>();

                var result = await repoFileUpload.GetAsync(x => x.Reference == request.Reference || x.Name == request.Reference && (bool)x.Active!, cancellationToken: cancellationToken);
                ThrowException.Exception.IfObjectClassNull(result, request.Reference!);

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
