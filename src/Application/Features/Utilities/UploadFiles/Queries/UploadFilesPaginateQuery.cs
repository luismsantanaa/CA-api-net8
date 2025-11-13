using Application.Features.Utilities.UploadFiles.Queries.Specs;
using Application.Features.Utilities.UploadFiles.Queries.Vm;
using AutoMapper;
using Domain.Entities.Shared;
using MediatR;
using Persistence.Pagination;
using Persistence.Repositories.Contracts;

namespace Application.Features.Utilities.UploadFiles.Queries
{
    public class UploadFilesPaginateQuery : PaginationBase, IRequest<PaginationVm<UploadedFileVm>>
    { }

    public class UploadFilesPaginateQueryHandler(
        IRepositoryFactory _repositoryFactory,
        IMapper _mapper) : IRequestHandler<UploadFilesPaginateQuery, PaginationVm<UploadedFileVm>>
    {

        public async Task<PaginationVm<UploadedFileVm>> Handle(UploadFilesPaginateQuery request, CancellationToken cancellationToken)
        {
            var fileUploadsSpecificationParams = new UploadFilesSpecificationParams
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                Search = request.Search,
                Sort = request.Sort,
            };

            var spec = new UploadFilesSpecification(fileUploadsSpecificationParams);
            var spectCount = new UploadFilesForCountingSpecification(fileUploadsSpecificationParams);

            var filesUpload = await _repositoryFactory.GetRepository<UploadedFile>().GetAllWithSpec(spec, cancellationToken);
            var totalProducts = await _repositoryFactory.GetRepository<UploadedFile>().CountAsync(spectCount, cancellationToken);

            var rounded = Math.Ceiling(Convert.ToDecimal(totalProducts) / Convert.ToDecimal(fileUploadsSpecificationParams.PageSize));
            var totalPages = Convert.ToInt32(rounded);

            var data = _mapper.Map<IReadOnlyList<UploadedFile>, IReadOnlyList<UploadedFileVm>>(filesUpload);

            var pagination = new PaginationVm<UploadedFileVm>
            {
                Count = totalProducts,
                Data = data,
                PageCount = totalPages,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return pagination;
        }
    }
}
