using Application.DTOs;
using Application.Features.Examples.Categories.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using MediatR;
using Microsoft.Extensions.Logging;
using Persistence.Caching.Contracts;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;

namespace Application.Features.Examples.Categories.Commands
{
    public class UpdateCategoryCommand : IRequest<Result<string>>
    {
        public required string Id { get; set; }
        public string? Name { get; set; }
        public string? Image { get; set; }
    }

    public class UpdateCategoryCommandHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        IMapper _mapper,
        ILogger<UpdateCategoryCommandHandler> _logger,
        IUnitOfWork _unitOfWork) : IRequestHandler<UpdateCategoryCommand, Result<string>>
    {

        public async Task<Result<string>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var repo = _unitOfWork.Repository<TestCategory>();

                var guid = request.Id!.StringToGuid();

                // Use GetByIdAsync for better performance when updating by ID (uses FindAsync with tracking)
                var entityToUpdate = await repo.GetByIdAsync(guid, cancellationToken)!;
                ThrowException.Exception.IfObjectClassNull(entityToUpdate, request.Id!);

                _mapper.Map(request, entityToUpdate, typeof(UpdateCategoryCommand), typeof(TestCategory));

                await repo.UpdateAsync(entityToUpdate, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var cacheKey = _cacheKeyService.GetListKey(typeof(CategoryVm).Name);
                await _cacheService.RemoveAsync(cacheKey);

                return Result<string>.Success(request.Id.ToString(), 1, ErrorMessage.UpdatedSuccessfully("Entity", entityToUpdate.Name!));
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorUpdating);
                _logger.LogError(ex, message);
                throw new InternalServerError(message, ex);
            }
        }
    }

}
