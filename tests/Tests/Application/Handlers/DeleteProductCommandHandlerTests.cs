using Application.Features.Examples.Products.Commands;
using Application.Features.Examples.Products.VMs;
using Domain.Entities.Examples;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Caching.Contracts;
using Persistence.Repositories.Contracts;
using Tests.Helpers;

namespace Tests.Application.Handlers
{
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICacheInvalidationService> _cacheInvalidationServiceMock;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cacheInvalidationServiceMock = new Mock<ICacheInvalidationService>();
            _logger = TestFixture.CreateLogger<DeleteProductCommandHandler>();
        }

        [Fact]
        public async Task Handle_WithValidId_ShouldDeleteSuccessfully()
        {
            // Arrange
            var handler = new DeleteProductCommandHandler(
                _cacheInvalidationServiceMock.Object,
                _logger,
                _unitOfWorkMock.Object);

            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var request = new DeleteProductCommand { Id = productId.ToString() };

            var existingProduct = new TestProduct
            {
                Id = productId,
                Name = "Product to Delete",
                CategoryId = categoryId
            };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _unitOfWorkMock.Setup(u => u.Repository<TestProduct>())
                .Returns(repositoryMock.Object);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().Be(productId.ToString());

            // Verify repository calls
            repositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);

            // Verify cache invalidation
            _cacheInvalidationServiceMock.Verify(
                c => c.InvalidateEntityCacheAsync<ProductVm>(categoryId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentProduct_ShouldThrowException()
        {
            // Arrange
            var handler = new DeleteProductCommandHandler(
                _cacheInvalidationServiceMock.Object,
                _logger,
                _unitOfWorkMock.Object);

            var productId = Guid.NewGuid();
            var request = new DeleteProductCommand { Id = productId.ToString() };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestProduct?)null);

            _unitOfWorkMock.Setup(u => u.Repository<TestProduct>())
                .Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithInvalidGuid_ShouldThrowException()
        {
            // Arrange
            var handler = new DeleteProductCommandHandler(
                _cacheInvalidationServiceMock.Object,
                _logger,
                _unitOfWorkMock.Object);

            var request = new DeleteProductCommand { Id = "invalid-guid" };

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await handler.Handle(request, CancellationToken.None));
        }
    }
}
