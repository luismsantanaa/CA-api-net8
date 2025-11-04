using Application.DTOs;
using Application.Features.Examples.Categories.VMs;
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

namespace Application.Features.Examples.Categories.Commands
{
    public class CreateCategoryCommand : IRequest<Result<string>>
    {
        public required string Name { get; set; }
        public string? Image { get; set; }
    }

    public class CreateCategoryCommandHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        IMapper _mapper,
        ILogger<CreateCategoryCommandHandler> _logger,
        IUnitOfWork _unitOfWork) : IRequestHandler<CreateCategoryCommand, Result<string>>
    {

        public async Task<Result<string>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Enrich log context with command-specific information
            using (LogContext.PushProperty("CategoryName", request.Name))
            {
                try
                {
                    _logger.LogInformation("Starting category creation. Name: {CategoryName}", request.Name);

                    var repo = _unitOfWork.Repository<TestCategory>();

                    var exist = await repo.GetFirstAsync(x => x.Name! == request.Name!, cancellationToken)!;
                    ThrowException.Exception.IfNotNull(exist, ErrorMessage.RecordExist);

                    var entityToAdd = _mapper.Map<TestCategory>(request);

                    await repo.AddAsync(entityToAdd, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var cacheKey = _cacheKeyService.GetListKey(typeof(CategoryVm).Name);
                    await _cacheService.RemoveAsync(cacheKey);

                    _logger.LogInformation("Category created successfully. CategoryId: {CategoryId}, Name: {CategoryName}", 
                        entityToAdd.Id, request.Name);

                    return Result<string>.Success(entityToAdd.Id.ToString(), 1, ErrorMessage.AddedSuccessfully("Category", request.Name!));
                }
                catch (Exception ex)
                {
                    var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorCreating);
                    
                    _logger.LogError(ex, 
                        "Error creating category. Name: {CategoryName}, Error: {ErrorMessage}", 
                        request.Name, message);
                    
                    throw new InternalServerError(message, ex);
                }
            }
        }
    }


}
