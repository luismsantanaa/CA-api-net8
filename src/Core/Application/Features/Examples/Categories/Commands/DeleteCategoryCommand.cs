using Application.DTOs;
using Application.Features.Examples.Categories.VMs;
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
    // DeleteCategoryCommand

    public class DeleteCategoryCommand : IRequest<Result<string>>
    {
        public required string Id { get; set; }
    }

    public class DeleteCategoryCommandHandler(
        ICacheKeyService _cacheKeyService,
        ICacheService _cacheService,
        ILogger<DeleteCategoryCommandHandler> _logger,
        IUnitOfWork _unitOfWork) : IRequestHandler<DeleteCategoryCommand, Result<string>>
    {

        public async Task<Result<string>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var repo = _unitOfWork.Repository<TestCategory>();

                var guid = Guid.Parse(request.Id!);

                var entityToDelete = await repo.GetByIdAsync(guid, cancellationToken)!;
                ThrowException.Exception.IfObjectClassNull(entityToDelete, request.Id!);

                entityToDelete.Active = false;

                await repo.UpdateAsync(entityToDelete, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var cacheKey = _cacheKeyService.GetListKey(typeof(CategoryVm).Name);
                await _cacheService.RemoveAsync(cacheKey);

                return Result<string>.Success(entityToDelete.Id.ToString(), 1, ErrorMessage.DeletedSuccessfully("Category", entityToDelete.Id.ToString()));
            }
            catch (Exception ex)
            {
                var message = ErrorMessageFormatter.Format(ex, ErrorMessage.ErrorDeleting);
                _logger.LogError(ex, message);
                throw new InternalServerError(message, ex);
            }
        }
    }

}
