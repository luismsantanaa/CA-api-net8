using Application.DTOs;
using Application.Features.Examples.Products.VMs;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Persistence.Repositories.Contracts;
using Serilog.Context;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Examples.Products.Commands
{

    public class DeleteProductCommand : IRequest<Result<string>>
    {
        public string? Id { get; set; }
    }

    public class DeleteProductCommandHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        ILogger<DeleteProductCommandHandler> _logger,
        IUnitOfWork _unitOfWork) : IRequestHandler<DeleteProductCommand, Result<string>>
    {

        public async Task<Result<string>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var productId = request.Id!.StringToGuid();
            
            // Enrich log context with command-specific information
            using (LogContext.PushProperty("ProductId", productId))
            {
                try
                {
                    _logger.LogInformation("Starting product deletion. ProductId: {ProductId}", productId);

                    var repo = _unitOfWork.Repository<TestProduct>();

                    var entityToDelete = await repo.GetByIdAsync(productId, cancellationToken)!;
                    ThrowException.Exception.IfObjectClassNull(entityToDelete, request.Id!);

                    // Store CategoryId before deletion for cache invalidation
                    var categoryId = entityToDelete.CategoryId;

                    await repo.DeleteAsync(entityToDelete, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Invalidate both generic list cache and category-specific cache
                    var genericCacheKey = _cacheKeyService.GetListKey(typeof(ProductVm).Name);
                    await _cacheService.RemoveAsync(genericCacheKey);
                    
                    var categoryCacheKey = _cacheKeyService.GetKey($"{typeof(ProductVm).Name}:Category", categoryId);
                    await _cacheService.RemoveAsync(categoryCacheKey);

                    _logger.LogInformation("Product deleted successfully. ProductId: {ProductId}", productId);

                    return Result<string>.Success(entityToDelete.Id.ToString(), 1, ErrorMessage.DeletedSuccessfully("Product", entityToDelete.Id.ToString()));
                }
                catch (Exception ex)
                {
                    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorDeleting);
                    
                    _logger.LogError(ex, 
                        "Error deleting product. ProductId: {ProductId}, Error: {ErrorMessage}", 
                        productId, message);
                    
                    throw new InternalServerError(message, ex);
                }
            }
        }
    }


}
