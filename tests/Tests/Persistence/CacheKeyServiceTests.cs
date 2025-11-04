using FluentAssertions;
using Persistence.Caching;

namespace Tests.Persistence
{
    /// <summary>
    /// Unit tests for CacheKeyService.
    /// Tests cache key generation for different scenarios (list, select list, single item, details).
    /// </summary>
    public class CacheKeyServiceTests
    {
        private readonly CacheKeyService _cacheKeyService;

        public CacheKeyServiceTests()
        {
            _cacheKeyService = new CacheKeyService();
        }

        [Fact]
        public void GetListKey_WithValidEntityName_ShouldReturnFormattedKey()
        {
            // Arrange
            var entityName = "ProductVm";

            // Act
            var result = _cacheKeyService.GetListKey(entityName);

            // Assert
            result.Should().Be("ProductVm:List");
        }

        [Fact]
        public void GetListKey_WithDifferentEntity_ShouldReturnDifferentKey()
        {
            // Arrange
            var entityName1 = "ProductVm";
            var entityName2 = "CategoryVm";

            // Act
            var result1 = _cacheKeyService.GetListKey(entityName1);
            var result2 = _cacheKeyService.GetListKey(entityName2);

            // Assert
            result1.Should().Be("ProductVm:List");
            result2.Should().Be("CategoryVm:List");
            result1.Should().NotBe(result2);
        }

        [Fact]
        public void GetSelectListKey_WithValidEntityName_ShouldReturnFormattedKey()
        {
            // Arrange
            var entityName = "ProductVm";

            // Act
            var result = _cacheKeyService.GetSelectListKey(entityName);

            // Assert
            result.Should().Be("ProductVm:SelectList");
        }

        [Fact]
        public void GetKey_WithEntityNameAndGuid_ShouldReturnFormattedKey()
        {
            // Arrange
            var entityName = "ProductVm";
            var pkId = Guid.NewGuid();

            // Act
            var result = _cacheKeyService.GetKey(entityName, pkId);

            // Assert
            result.Should().Be($"ProductVm:{pkId}");
        }

        [Fact]
        public void GetKey_WithEntityNameAndStringId_ShouldReturnFormattedKey()
        {
            // Arrange
            var entityName = "ProductVm";
            var pkId = "12345";

            // Act
            var result = _cacheKeyService.GetKey(entityName, pkId);

            // Assert
            result.Should().Be("ProductVm:12345");
        }

        [Fact]
        public void GetKey_WithCategorySpecificPrefix_ShouldReturnFormattedKey()
        {
            // Arrange
            var entityName = "ProductVm:Category";
            var categoryId = Guid.NewGuid();

            // Act
            var result = _cacheKeyService.GetKey(entityName, categoryId);

            // Assert
            result.Should().Be($"ProductVm:Category:{categoryId}");
        }

        [Fact]
        public void GetDetailsKey_WithEntityNameAndGuid_ShouldReturnFormattedKey()
        {
            // Arrange
            var entityName = "ProductVm";
            var pkId = Guid.NewGuid();

            // Act
            var result = _cacheKeyService.GetDetailsKey(entityName, pkId);

            // Assert
            result.Should().Be($"ProductVmDetails:{pkId}");
        }

        [Fact]
        public void GetDetailsKey_WithDifferentEntities_ShouldReturnUniqueKeys()
        {
            // Arrange
            var entityName1 = "ProductVm";
            var entityName2 = "CategoryVm";
            var pkId = Guid.NewGuid();

            // Act
            var result1 = _cacheKeyService.GetDetailsKey(entityName1, pkId);
            var result2 = _cacheKeyService.GetDetailsKey(entityName2, pkId);

            // Assert
            result1.Should().Be($"ProductVmDetails:{pkId}");
            result2.Should().Be($"CategoryVmDetails:{pkId}");
            result1.Should().NotBe(result2);
        }

        [Fact]
        public void GetKey_WithDifferentIds_ShouldReturnDifferentKeys()
        {
            // Arrange
            var entityName = "ProductVm";
            var pkId1 = Guid.NewGuid();
            var pkId2 = Guid.NewGuid();

            // Act
            var result1 = _cacheKeyService.GetKey(entityName, pkId1);
            var result2 = _cacheKeyService.GetKey(entityName, pkId2);

            // Assert
            result1.Should().NotBe(result2);
            result1.Should().Contain(pkId1.ToString());
            result2.Should().Contain(pkId2.ToString());
        }
    }
}

