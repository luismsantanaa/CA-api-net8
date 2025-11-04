using Application.DTOs;
using Application.Features.Examples.Products.Queries;
using Application.Features.Examples.Products.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Repositories.Contracts;
using Persistence.Specification.Contracts;
using Tests.Helpers;

namespace Tests.Application.Handlers
{
    /// <summary>
    /// Unit tests for GetProductByIdQueryHandler.
    /// Tests product retrieval by ID, mapping to view model, and error scenarios.
    /// </summary>
    public class GetProductByIdQueryHandlerTests
    {
        private readonly Mock<IRepositoryFactory> _repositoryFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ILogger<GetProductByIdQueryHandler> _logger;

        public GetProductByIdQueryHandlerTests()
        {
            _repositoryFactoryMock = new Mock<IRepositoryFactory>();
            _mapperMock = new Mock<IMapper>();
            _logger = TestFixture.CreateLogger<GetProductByIdQueryHandler>();
        }

        [Fact]
        public async Task Handle_WithValidProductId_ShouldReturnProductVm()
        {
            // Arrange
            var handler = new GetProductByIdQueryHandler(
                _mapperMock.Object,
                _logger,
                _repositoryFactoryMock.Object);

            var productId = Guid.NewGuid();
            var request = new GetProductByIdQuery
            {
                PkId = productId.ToString()
            };

            var productEntity = new TestProduct
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Description",
                Price = 99.99,
                CategoryId = Guid.NewGuid()
            };

            var productVm = new ProductVm(
                Id: productId,
                Title: "Test Product",
                Description: "Test Description",
                Image: null,
                Price: 99.99,
                CategoryId: Guid.NewGuid(),
                CategoryName: null);

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            repositoryMock.Setup(r => r.GetOneWithSpec(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(productEntity);

            _repositoryFactoryMock.Setup(r => r.GetRepository<TestProduct>()).Returns(repositoryMock.Object);
            _mapperMock.Setup(m => m.Map<ProductVm>(productEntity)).Returns(productVm);

            // Act
            var result = await handler.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().NotBeNull();
            result.Items!.Id.Should().Be(productVm.Id);
            result.Items.Title.Should().Be(productVm.Title);
            result.Items.Description.Should().Be(productVm.Description);
            result.Items.Price.Should().Be(productVm.Price);
            
            repositoryMock.Verify(r => r.GetOneWithSpec(
                It.IsAny<ISpecification<TestProduct>>(), 
                It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ProductVm>(productEntity), Times.Once);
        }

        [Fact]
        public async Task Handle_WithNonExistentProductId_ShouldThrowException()
        {
            // Arrange
            var handler = new GetProductByIdQueryHandler(
                _mapperMock.Object,
                _logger,
                _repositoryFactoryMock.Object);

            var productId = Guid.NewGuid();
            var request = new GetProductByIdQuery
            {
                PkId = productId.ToString()
            };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            repositoryMock.Setup(r => r.GetOneWithSpec(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TestProduct?)null); // Product doesn't exist

            _repositoryFactoryMock.Setup(r => r.GetRepository<TestProduct>()).Returns(repositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithInvalidGuidFormat_ShouldThrowException()
        {
            // Arrange
            var handler = new GetProductByIdQueryHandler(
                _mapperMock.Object,
                _logger,
                _repositoryFactoryMock.Object);

            var request = new GetProductByIdQuery
            {
                PkId = "invalid-guid-format"
            };

            // Act & Assert
            // The StringToGuid extension will throw an exception for invalid format
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await handler.Handle(request, CancellationToken.None));
        }
    }
}

