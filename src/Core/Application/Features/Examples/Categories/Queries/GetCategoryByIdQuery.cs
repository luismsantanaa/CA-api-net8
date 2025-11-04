using System.Linq.Expressions;
using Application.DTOs;
using Application.Features.Examples.Categories.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Repositories.Contracts;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Examples.Categories.Queries
{

    public class GetCategoryByIdQuery : IRequest<Result<CategoryVm>>
    {
        public string? PkId { get; set; }
    }

    public class GetCategoryByIdQueryHandler(
        IMapper _mapper,
        ILogger<GetCategoryByIdQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryVm>>
    {

        public async Task<Result<CategoryVm>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<TestCategory, bool>> criterial = x => x.Id == request.PkId!.StringToGuid();

            var repo = _repositoryFactory.GetRepository<TestCategory>();

            var dbData = await repo.GetFirstAsync(criterial, cancellationToken)!;
            ThrowException.Exception.IfObjectClassNull(dbData, request.PkId!);

            var result = _mapper.Map<CategoryVm>(dbData);

            return Result<CategoryVm>.Success(result, 1);
        }
    }
}
