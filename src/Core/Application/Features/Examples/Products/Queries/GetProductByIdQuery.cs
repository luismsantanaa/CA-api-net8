using Application.DTOs;
using Application.Features.Examples.Products.Queries.Specs;
using Application.Features.Examples.Products.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Repositories.Contracts;
using Serilog.Context;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Examples.Products.Queries
{
    public class GetProductByIdQuery : IRequest<Result<ProductVm>>
    {
        public string? PkId { get; set; }
    }

    public class GetProductByIdQueryHandler(
        IMapper _mapper,
        ILogger<GetProductByIdQueryHandler> _logger,
        IRepositoryFactory _repositoryFactory) : IRequestHandler<GetProductByIdQuery, Result<ProductVm>>
    {

        public async Task<Result<ProductVm>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var productId = request.PkId!.StringToGuid();
            
            // Enrich log context with query-specific information
            using (LogContext.PushProperty("ProductId", productId))
            {
                _logger.LogDebug("Fetching product by ID. ProductId: {ProductId}", productId);

                var spec = new GetProductsById(productId);

                var repo = _repositoryFactory.GetRepository<TestProduct>();

                var dbData = await repo.GetOneWithSpec(spec, cancellationToken)!;
                ThrowException.Exception.IfObjectClassNull(dbData, request.PkId!);

                var result = _mapper.Map<ProductVm>(dbData);

                _logger.LogDebug("Product retrieved successfully. ProductId: {ProductId}", productId);

                return Result<ProductVm>.Success(result, 1);
            }
        }
    }
}
