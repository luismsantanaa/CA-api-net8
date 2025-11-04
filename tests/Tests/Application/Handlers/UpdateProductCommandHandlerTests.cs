using Application.DTOs;
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
        private readonly Mock<ICacheKeyService> _cacheKeyServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _cacheKeyServiceMock = new Mock<ICacheKeyService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _mapperMock = new Mock<IMapper>();
            _logger = TestFixture.CreateLogger<UpdateProductCommandHandler>();
        }

        [Fact]
        public async Task Handle_WithValidRequest_ShouldUpdateProductSuccessfully()
        {
            // Arrange
            var handler = new UpdateProductCommandHandler(
                _cacheKeyServiceMock.Object,
                _cacheServiceMock.Object,
                _mapperMock.Object,
                _logger,
                _unitOfWorkMock.Object);

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
            repositoryMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProduct);

            _unitOfWorkMock.Setup(u => u.Repository<TestProduct>()).Returns(repositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            _cacheKeyServiceMock.Setup(c => c.GetListKey(typeof(ProductVm).Name)).Returns("ProductVm:List");
            _cacheKeyServiceMock.Setup(c => c.GetKey(It.IsAny<string>(), It.IsAny<object>())).Returns("ProductVm:Category:xxx");

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
            
            // Verify cache invalidation was called (generic list + new category + old category if changed)
            _cacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task Handle_WithNonExistentProduct_ShouldThrowException()
        {
            // Arrange
            var handler = new UpdateProductCommandHandler(
                _cacheKeyServiceMock.Object,
                _cacheServiceMock.Object,
                _mapperMock.Object,
                _logger,
                _unitOfWorkMock.Object);

            var productId = Guid.NewGuid();
            var request = new UpdateProductCommand
            {
                Id = productId.ToString(),
                Name = "Updated Product",
                Price = 199.99,
                CategoryId = Guid.NewGuid()
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

