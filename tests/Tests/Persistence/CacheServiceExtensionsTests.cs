using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence.Caching.Contracts;
using Persistence.Caching.Extensions;
using Shared.Exceptions;
using Tests.Helpers;

namespace Tests.Persistence
{
    /// <summary>
    /// Unit tests for CacheServiceExtensions.
    /// Tests GetOrSetAsync extension method behavior with cache hits, misses, and error scenarios.
    /// </summary>
    public class CacheServiceExtensionsTests
    {
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly ILogger _logger;

        public CacheServiceExtensionsTests()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _logger = TestFixture.CreateLogger<CacheServiceExtensionsTests>();
        }

        [Fact]
        public async Task GetOrSetAsync_WithCacheHit_ShouldReturnCachedValue()
        {
            // Arrange
            var cacheKey = "ProductVm:List";
            var cachedValue = new List<string> { "Item1", "Item2" };

            _cacheServiceMock.Setup(c => c.GetAsync<List<string>>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedValue);

            Func<Task<List<string>>> getItemCallback = async () => 
            {
                await Task.Delay(1);
                return new List<string> { "Item3" };
            };

            // Act
            var result = await _cacheServiceMock.Object.GetOrSetAsync(
                cacheKey, 
                getItemCallback, 
                cancellationToken: CancellationToken.None,
                logger: _logger);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(cachedValue);
            _cacheServiceMock.Verify(c => c.GetAsync<List<string>>(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetOrSetAsync_WithCacheMiss_ShouldCallCallbackAndCacheResult()
        {
            // Arrange
            var cacheKey = "ProductVm:List";
            var fetchedValue = new List<string> { "Item1", "Item2" };

            _cacheServiceMock.Setup(c => c.GetAsync<List<string>>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<string>?)null); // Cache miss

            Func<Task<List<string>>> getItemCallback = async () => 
            {
                await Task.Delay(1);
                return fetchedValue;
            };

            _cacheServiceMock.Setup(c => c.SetAsync(
                cacheKey, 
                fetchedValue, 
                It.IsAny<TimeSpan?>(), 
                null, 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cacheServiceMock.Object.GetOrSetAsync(
                cacheKey, 
                getItemCallback, 
                cancellationToken: CancellationToken.None,
                logger: _logger);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(fetchedValue);
            _cacheServiceMock.Verify(c => c.GetAsync<List<string>>(cacheKey, It.IsAny<CancellationToken>()), Times.Once);
            _cacheServiceMock.Verify(c => c.SetAsync(
                cacheKey, 
                fetchedValue, 
                It.IsAny<TimeSpan?>(), 
                null, 
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetOrSetAsync_WithCallbackThrowingNotFoundException_ShouldRethrowException()
        {
            // Arrange
            var cacheKey = "ProductVm:123";
            var notFoundException = new NotFoundException("Product", Guid.NewGuid());

            _cacheServiceMock.Setup(c => c.GetAsync<object>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);

            Func<Task<object>> getItemCallback = async () => 
            {
                await Task.Delay(1);
                throw notFoundException;
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                async () => await _cacheServiceMock.Object.GetOrSetAsync(
                    cacheKey, 
                    getItemCallback, 
                    cancellationToken: CancellationToken.None,
                    logger: _logger));

            exception.Should().Be(notFoundException);
        }

        [Fact]
        public async Task GetOrSetAsync_WithCallbackThrowingBadRequestException_ShouldRethrowException()
        {
            // Arrange
            var cacheKey = "ProductVm:123";
            var badRequestException = new BadRequestException("Invalid request");

            _cacheServiceMock.Setup(c => c.GetAsync<object>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);

            Func<Task<object>> getItemCallback = async () => 
            {
                await Task.Delay(1);
                throw badRequestException;
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(
                async () => await _cacheServiceMock.Object.GetOrSetAsync(
                    cacheKey, 
                    getItemCallback, 
                    cancellationToken: CancellationToken.None,
                    logger: _logger));

            exception.Should().Be(badRequestException);
        }

        [Fact]
        public async Task GetOrSetAsync_WithCallbackThrowingGenericException_ShouldWrapInInternalServerError()
        {
            // Arrange
            var cacheKey = "ProductVm:123";
            var genericException = new Exception("Database connection failed");

            _cacheServiceMock.Setup(c => c.GetAsync<object>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((object?)null);

            Func<Task<object>> getItemCallback = async () => 
            {
                await Task.Delay(1);
                throw genericException;
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InternalServerError>(
                async () => await _cacheServiceMock.Object.GetOrSetAsync(
                    cacheKey, 
                    getItemCallback, 
                    cancellationToken: CancellationToken.None,
                    logger: _logger));

            exception.Message.Should().Contain($"Cache operation failed for key '{cacheKey}'");
            exception.InnerException.Should().Be(genericException);
        }

        [Fact]
        public async Task GetOrSetAsync_WithNullCallbackResult_ShouldNotCache()
        {
            // Arrange
            var cacheKey = "ProductVm:List";

            _cacheServiceMock.Setup(c => c.GetAsync<List<string>>(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<string>?)null);

            Func<Task<List<string>?>> getItemCallback = async () => 
            {
                await Task.Delay(1);
                return null; // Null result from callback
            };

            // Act
            var result = await _cacheServiceMock.Object.GetOrSetAsync(
                cacheKey, 
                getItemCallback, 
                cancellationToken: CancellationToken.None,
                logger: _logger);

            // Assert
            result.Should().BeNull();
            _cacheServiceMock.Verify(c => c.SetAsync(
                It.IsAny<string>(), 
                It.IsAny<object>(), 
                It.IsAny<TimeSpan?>(), 
                null, 
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

