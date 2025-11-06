// tests/Tests/Application/Handlers/GetAllProductsQueryHandlerTests.cs
using Application.Features.Examples.Products.Queries;
using Application.Features.Examples.Products.VMs;
using AutoMapper;
using Domain.Entities.Examples;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Caching.Contracts;
using Persistence.Caching.Extensions;
using Persistence.Repositories.Contracts;
using Persistence.Specification.Contracts;
using Tests.Helpers;

namespace Tests.Application.Handlers
{
    public class GetAllProductsQueryHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryFactory> _repositoryFactoryMock;
        private readonly Mock<ICacheKeyService> _cacheKeyServiceMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly ILogger<GetAllProductsQueryHandler> _logger;

        public GetAllProductsQueryHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryFactoryMock = new Mock<IRepositoryFactory>();
            _cacheKeyServiceMock = new Mock<ICacheKeyService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _logger = TestFixture.CreateLogger<GetAllProductsQueryHandler>();
        }

        [Fact]
        public async Task Handle_WithoutCache_ShouldReturnAllProducts()
        {
            // Arrange
            var handler = new GetAllProductsQueryHandler(
               _cacheKeyServiceMock.Object,
               _cacheServiceMock.Object,
                _mapperMock.Object,
                _logger,
                _repositoryFactoryMock.Object);

            var products = new List<TestProduct>
            {
                new() { Id = Guid.NewGuid(), Name = "Product 1" },
                new() { Id = Guid.NewGuid(), Name = "Product 2" }
            };

            var productVms = new List<ProductVm>
            {
                new(products[0].Id, "Product 1", "", null, 0, Guid.Empty, null),
                new(products[1].Id, "Product 2", "", null, 0, Guid.Empty, null)
            };

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            // GetAllWithSpec is used instead of GetAllAsync
            repositoryMock.Setup(r => r.GetAllWithSpec(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(products);

            _repositoryFactoryMock.Setup(f => f.GetRepository<TestProduct>())
                .Returns(repositoryMock.Object);

            _mapperMock.Setup(m => m.Map<List<ProductVm>>(It.IsAny<IReadOnlyList<TestProduct>>()))
                .Returns(productVms);

            // Act
            var result = await handler.Handle(new GetAllProductsQuery { EnableCache = false }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().NotBeNull();
            result.Items.Should().HaveCount(2);

            // Verify repository was called
            repositoryMock.Verify(r => r.GetAllWithSpec(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithCache_ShouldReturnCachedResults()
        {
            // Arrange
            var handler = new GetAllProductsQueryHandler(
                _cacheKeyServiceMock.Object,
               _cacheServiceMock.Object,
                _mapperMock.Object,
                _logger,
                _repositoryFactoryMock.Object);

            var cachedProductVms = new List<ProductVm>
            {
                new(Guid.NewGuid(), "Cached Product 1", "", null, 0, Guid.Empty, null),
                new(Guid.NewGuid(), "Cached Product 2", "", null, 0, Guid.Empty, null)
            };

            var cacheKey = "products_list";
            _cacheKeyServiceMock.Setup(c => c.GetListKey(typeof(ProductVm).Name))
                .Returns(cacheKey);

            // Mock GetAsync to return cached data (simulating cache hit)
            // GetOrSetAsync is an extension method that uses GetAsync internally
            _cacheServiceMock.Setup(c => c.GetAsync<List<ProductVm>>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedProductVms);

            // Act
            var result = await handler.Handle(new GetAllProductsQuery { EnableCache = true }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().NotBeNull();
            result.Items.Should().HaveCount(2);

            // Verify cache was checked
            _cacheServiceMock.Verify(c => c.GetAsync<List<ProductVm>>(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify repository was NOT called (because data was in cache)
            _repositoryFactoryMock.Verify(f => f.GetRepository<TestProduct>(), Times.Never);
        }

        [Fact]
        public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
        {
            // Arrange
            var handler = new GetAllProductsQueryHandler(
               _cacheKeyServiceMock.Object,
               _cacheServiceMock.Object,
               _mapperMock.Object,
               _logger,
               _repositoryFactoryMock.Object);

            var emptyProducts = new List<TestProduct>(); // Empty list, not null
            var emptyProductVms = new List<ProductVm>();

            var repositoryMock = new Mock<IGenericRepository<TestProduct>>();
            // Return empty list (not null) so IfClassNull doesn't throw
            repositoryMock.Setup(r => r.GetAllWithSpec(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyProducts);

            _repositoryFactoryMock.Setup(f => f.GetRepository<TestProduct>())
                .Returns(repositoryMock.Object);

            _mapperMock.Setup(m => m.Map<List<ProductVm>>(It.IsAny<IReadOnlyList<TestProduct>>()))
                .Returns(emptyProductVms);

            // Act
            var result = await handler.Handle(new GetAllProductsQuery { EnableCache = false }, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().NotBeNull();
            result.Items.Should().BeEmpty();

            // Verify repository was called
            repositoryMock.Verify(r => r.GetAllWithSpec(It.IsAny<ISpecification<TestProduct>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
