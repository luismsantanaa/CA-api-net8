using Application.Features.Examples.Products.Commands;
using Application.Features.Examples.Products.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Caching.Contracts;
using Persistence.Repositories.Contracts;
using Tests.Helpers;

namespace Tests.Application.Handlers
{
    /// <summary>
    /// Unit tests for UpdateProductCommandHandler.
    /// Tests product update logic, validation, cache invalidation, and error scenarios.
    /// </summary>
    public class UpdateProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICacheInvalidationService> _cacheInvalidationServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cacheInvalidationServiceMock = new Mock<ICacheInvalidationService>();
            _mapperMock = new Mock<IMapper>();
            _logger = TestFixture.CreateLogger<UpdateProductCommandHandler>();
        }

        [Fact]
        public async Task Handle_WithValidRequest_ShouldUpdateProductSuccessfully()
        {
            // Arrange
            var handler = new UpdateProductCommandHandler(
                _cacheInvalidationServiceMock.Object,  // ICacheInvalidationService first
                _mapperMock.Object,                     // IMapper second
                _logger,                                // ILogger third
                _unitOfWorkMock.Object);                // IUnitOfWork fourth

            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var oldCategoryId = Guid.NewGuid();

            var request = new UpdateProductCommand
            {
                Id = productId.ToString(),
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 199.99,
                CategoryId = categoryId
            };

            var existingProduct = new TestProduct
            {
                Id = productId,
                Name = "Original Product",
                Description = "Original Description",
                Price = 99.99,
                CategoryId = oldCategoryId
            };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())!)
                .ReturnsAsync(existingProduct);

            _unitOfWorkMock.Setup(u => u.Repository<TestProduct>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().Be(productId.ToString());

            // Verify mapper was called to update entity (actual property updates happen in mapper)
            // The mapper.Map with UpdateProductCommand and TestProduct will update the entity
            _mapperMock.Verify(m => m.Map(
                request,
                existingProduct,
                typeof(UpdateProductCommand),
                typeof(TestProduct)), Times.Once);

            repositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Verify cache invalidation was called (new category cache)
            _cacheInvalidationServiceMock.Verify(c => c.InvalidateEntityCacheAsync<ProductVm>(request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            // If category changed, old category cache should also be invalidated
            _cacheInvalidationServiceMock.Verify(c => c.InvalidateEntityCacheAsync<ProductVm>(oldCategoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentProduct_ShouldThrowException()
        {
            // Arrange
            var handler = new UpdateProductCommandHandler(
                _cacheInvalidationServiceMock.Object,  // ICacheInvalidationService first
                _mapperMock.Object,                     // IMapper second
                _logger,                                // ILogger third
                _unitOfWorkMock.Object);                // IUnitOfWork fourth

            var productId = Guid.NewGuid();
            var request = new UpdateProductCommand
            {
                Id = productId.ToString(),
                Name = "Updated Product",
                Price = 199.99,
                CategoryId = Guid.NewGuid()
            };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
#pragma warning disable CS8620 // Nullability mismatch in test mock setup
            repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())!)
                .ReturnsAsync((TestProduct?)null); // Product doesn't exist
#pragma warning restore CS8620

            _unitOfWorkMock.Setup(u => u.Repository<TestProduct>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await handler.Handle(request, CancellationToken.None));
        }
    }
}

