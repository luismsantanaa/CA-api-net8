using Application.DTOs;
using Application.Features.Examples.Products.Queries.Specs;
using Application.Features.Examples.Products.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Repositories.Contracts;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Examples.Products.Queries
{

    public class GetProductWithCategoryByIdQuery : IRequest<Result<ProductWithCategoryVm>>
    {
        public string? Id { get; set; }
    }

    public class GetProductWithCategoryByIdQueryHandler(
        IMapper _mapper,
        ILogger<GetProductWithCategoryByIdQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetProductWithCategoryByIdQuery, Result<ProductWithCategoryVm>>
    {

        public async Task<Result<ProductWithCategoryVm>> Handle(GetProductWithCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var spec = new GetProductWithCategory(request.Id!.StringToGuid());

            var repo = _repositoryFactory.GetRepository<TestProduct>();

            var dbData = await repo.GetOneWithSpec(spec, cancellationToken)!;
            ThrowException.Exception.IfObjectClassNull(dbData, request.Id!);

            var result = _mapper.Map<ProductWithCategoryVm>(dbData);

            return Result<ProductWithCategoryVm>.Success(result, 1);
        }
    }

}
