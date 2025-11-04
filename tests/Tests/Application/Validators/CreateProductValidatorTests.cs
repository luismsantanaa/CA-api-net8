using Application.Features.Examples.Products.Commands;
using Application.Features.Examples.Products.Commands.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Persistence.Repositories.Contracts;

namespace Tests.Application.Validators
{
    /// <summary>
    /// Unit tests for CreateProductValidator.
    /// Tests validation rules for product creation, including name, price, category, and database validations.
    /// </summary>
    public class CreateProductValidatorTests
    {
        private readonly CreateProductValidator _validator;
        private readonly Mock<IRepositoryFactory> _repositoryFactoryMock;

        public CreateProductValidatorTests()
        {
            _repositoryFactoryMock = new Mock<IRepositoryFactory>();
            _validator = new CreateProductValidator(_repositoryFactoryMock.Object);
        }

        [Fact]
        public async Task Validate_WithValidProduct_ShouldPass()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Valid Product Name",
                Description = "Valid description",
                Price = 99.99,
                CategoryId = categoryId
            };

            // Setup repository mock to return true for category exists check
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyName_ShouldFail()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = string.Empty,
                Description = "Description",
                Price = 99.99,
                CategoryId = categoryId
            };

            // Setup repository mock (even if validation fails on name, category check might still run)
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public async Task Validate_WithNullName_ShouldFail()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = null,
                Description = "Description",
                Price = 99.99,
                CategoryId = categoryId
            };

            // Setup repository mock
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public async Task Validate_WithNegativePrice_ShouldFail()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Product Name",
                Description = "Description",
                Price = -10.0,
                CategoryId = categoryId
            };

            // Setup repository mock
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public async Task Validate_WithZeroPrice_ShouldFail()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Product Name",
                Description = "Description",
                Price = 0,
                CategoryId = categoryId
            };

            // Setup repository mock
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public async Task Validate_WithEmptyGuidCategoryId_ShouldFail()
        {
            // Arrange
            var command = new CreateProductCommand
            {
                Name = "Product Name",
                Description = "Description",
                Price = 99.99,
                CategoryId = Guid.Empty
            };

            // Setup repository mock - Empty guid will fail validation before reaching async check
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(Guid.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
        }

        [Fact]
        public async Task Validate_WithNameTooLong_ShouldFail()
        {
            // Arrange - Product name exceeding maximum length (max is 50 per validator)
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = new string('A', 51), // Exceeds MaximumLength(50)
                Description = "Description",
                Price = 99.99,
                CategoryId = categoryId
            };

            // Setup repository mock
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public async Task Validate_WithValidPrice_ShouldPass()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Valid Product Name",
                Description = "Valid Description",
                Price = 0.01, // Minimum valid price
                CategoryId = categoryId
            };

            // Setup repository mock to return true for category exists check
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Price);
        }

        [Fact]
        public async Task Validate_WithNonExistentCategory_ShouldFail()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var command = new CreateProductCommand
            {
                Name = "Valid Product Name",
                Description = "Valid Description",
                Price = 99.99,
                CategoryId = categoryId
            };

            // Setup repository mock to return false (category doesn't exist)
            var categoryRepoMock = new Mock<IGenericRepository<Domain.Entities.Examples.TestCategory>>();
            categoryRepoMock.Setup(r => r.ExistsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _repositoryFactoryMock.Setup(r => r.GetRepository<Domain.Entities.Examples.TestCategory>())
                .Returns(categoryRepoMock.Object);

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CategoryId);
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("No Existe en la BD"));
        }
    }
}

