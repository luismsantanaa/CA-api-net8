using Application.DTOs;
using Application.Features.Examples.Products.Commands;
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
    /// Unit tests for DeleteProductCommandHandler.
    /// Tests product deletion logic, cache invalidation, and error scenarios.
    /// </summary>
    public class DeleteProductCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICacheKeyService> _cacheKeyServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cacheKeyServiceMock = new Mock<ICacheKeyService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _logger = TestFixture.CreateLogger<DeleteProductCommandHandler>();
        }

        [Fact]
        public async Task Handle_WithValidRequest_ShouldDeleteProductSuccessfully()
        {
            // Arrange
            var handler = new DeleteProductCommandHandler(
                _cacheKeyServiceMock.Object,
                _cacheServiceMock.Object,
                _logger,
                _unitOfWorkMock.Object);

            var productId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var request = new DeleteProductCommand
            {
                Id = productId.ToString()
            };

            var existingProduct = new TestProduct
            {
                Id = productId,
                Name = "Product to Delete",
                CategoryId = categoryId
            };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<TestProduct>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.Repository<TestProduct>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _cacheKeyServiceMock.Setup(c => c.GetListKey("ProductVm")).Returns("ProductVm:List");
            _cacheKeyServiceMock.Setup(c => c.GetKey(It.IsAny<string>(), categoryId)).Returns("ProductVm:Category:xxx");

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().Be(productId.ToString());
            
            repositoryMock.Verify(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
            repositoryMock.Verify(r => r.DeleteAsync(existingProduct, It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify cache invalidation was called (generic list + category-specific)
            _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_WithNonExistentProduct_ShouldThrowException()
        {
            // Arrange
            var handler = new DeleteProductCommandHandler(
                _cacheKeyServiceMock.Object,
                _cacheServiceMock.Object,
                _logger,
                _unitOfWorkMock.Object);

            var productId = Guid.NewGuid();
            var request = new DeleteProductCommand
            {
                Id = productId.ToString()
            };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestProduct?)null); // Product doesn't exist

            _unitOfWorkMock.Setup(u => u.Repository<TestProduct>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await handler.Handle(request, CancellationToken.None));
        }
    }
}

