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
    /// <summary>
    /// Command to create a new product in the system
    /// </summary>
    public class CreateProductCommand : IRequest<Result<string>>
    {
        /// <summary>
        /// Product name
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Product description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Product image URL or path
        /// </summary>
        public string? Image { get; set; }
        
        /// <summary>
        /// Product price
        /// </summary>
        public double Price { get; set; }
        
        /// <summary>
        /// Category identifier for the product
        /// </summary>
        public Guid CategoryId { get; set; }
    }

    /// <summary>
    /// Handler for CreateProductCommand.
    /// Validates product name uniqueness, creates the product entity, and invalidates cache.
    /// </summary>
    public class CreateProductCommandHandler(
        ICacheInvalidationService _cacheInvalidationService,
        IMapper _mapper,
        ILogger<CreateProductCommandHandler> _logger,
        IUnitOfWork _unitOfWork) : IRequestHandler<CreateProductCommand, Result<string>>
    {
        /// <summary>
        /// Handles the product creation command
        /// </summary>
        /// <param name="request">The create product command containing product details</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>Result containing the created product ID or error information</returns>
        public async Task<Result<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Enrich log context with command-specific information
            using (LogContext.PushProperty("ProductName", request.Name))
            using (LogContext.PushProperty("CategoryId", request.CategoryId))
            {
                try
                {
                    _logger.LogInformation("Starting product creation. Name: {ProductName}, CategoryId: {CategoryId}", 
                        request.Name, request.CategoryId);

                    var repo = _unitOfWork.Repository<TestProduct>();

                    var exist = await repo.GetFirstAsync(x => x.Name! == request.Name!, cancellationToken)!;
                    ThrowException.Exception.IfNotNull(exist, ErrorMessage.RecordExist);

                    var entityToAdd = _mapper.Map<TestProduct>(request);

                    await repo.AddAsync(entityToAdd, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    // Invalidate both generic list cache and category-specific cache
                    await _cacheInvalidationService.InvalidateEntityCacheAsync<ProductVm>(request.CategoryId, cancellationToken);

                    _logger.LogInformation("Product created successfully. ProductId: {ProductId}, Name: {ProductName}", 
                        entityToAdd.Id, request.Name);

                    return ResultExtensions.CreatedSuccessfully(entityToAdd.Id, "Product", request.Name);
                }
                catch (Exception ex)
                {
                    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
                    
                    _logger.LogError(ex, 
                        "Error creating product. Name: {ProductName}, CategoryId: {CategoryId}, Error: {ErrorMessage}", 
                        request.Name, request.CategoryId, message);
                    
                    throw new InternalServerError(message, ex);
                }
            }
        }
    }

}
