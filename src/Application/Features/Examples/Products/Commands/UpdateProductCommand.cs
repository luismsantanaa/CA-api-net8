using Application.DTOs;
using Application.Features.Examples.Products.VMs;
using AutoMapper;
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

    public class UpdateProductCommand : IRequest<Result<string>>
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public double Price { get; set; }
        public Guid CategoryId { get; set; }
    }

    public class UpdateProductCommandHandler(
        ICacheInvalidationService _cacheInvalidationService,
        IMapper _mapper,
        ILogger<UpdateProductCommandHandler> _logger,
        IUnitOfWork _unitOfWork) : IRequestHandler<UpdateProductCommand, Result<string>>
    {

        public async Task<Result<string>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var productId = request.Id!.StringToGuid();
            
            // Enrich log context with command-specific information
            using (LogContext.PushProperty("ProductId", productId))
            using (LogContext.PushProperty("ProductName", request.Name))
            using (LogContext.PushProperty("CategoryId", request.CategoryId))
            {
                try
                {
                    _logger.LogInformation("Starting product update. ProductId: {ProductId}, Name: {ProductName}", 
                        productId, request.Name);

                    var repo = _unitOfWork.Repository<TestProduct>();

                    // Use GetByIdAsync for better performance when updating by ID (uses FindAsync with tracking)
                    var entityToUpdate = await repo.GetByIdAsync(productId, cancellationToken)!;
                    ThrowException.Exception.IfObjectClassNull(entityToUpdate, request.Id!);

                    _mapper.Map(request, entityToUpdate, typeof(UpdateProductCommand), typeof(TestProduct));

                    await repo.UpdateAsync(entityToUpdate, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Invalidate both generic list cache and category-specific cache
                    await _cacheInvalidationService.InvalidateEntityCacheAsync<ProductVm>(request.CategoryId, cancellationToken);
                    
                    // If category changed, also invalidate old category cache
                    if (entityToUpdate.CategoryId != request.CategoryId)
                    {
                        await _cacheInvalidationService.InvalidateEntityCacheAsync<ProductVm>(entityToUpdate.CategoryId, cancellationToken);
                    }

                    _logger.LogInformation("Product updated successfully. ProductId: {ProductId}, Name: {ProductName}", 
                        productId, entityToUpdate.Name);

                    return ResultExtensions.UpdatedSuccessfully(request.Id!, "Product", entityToUpdate.Name);
                }
                catch (Exception ex)
                {
                    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorUpdating);
                    
                    _logger.LogError(ex, 
                        "Error updating product. ProductId: {ProductId}, Name: {ProductName}, Error: {ErrorMessage}", 
                        productId, request.Name, message);
                    
                    throw new InternalServerError(message, ex);
                }
            }
        }
    }

}
