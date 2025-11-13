using Application.Features.Utilities.UploadFiles.Comands.VoidFile;
using Domain.Entities.Shared;
using FluentAssertions;
using Moq;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using Xunit;

namespace Tests.Application.Utilities
{
    public class VoidUploadedFileCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<UploadedFile>> _repositoryMock;
        private readonly Mock<Shared.Services.Contracts.IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<global::Persistence.Caching.Contracts.ICacheInvalidationService> _cacheInvalidationServiceMock;
        private readonly Mock<Microsoft.Extensions.Logging.ILogger<VoidUploadedFileCommandHandler>> _loggerMock;
        private readonly VoidUploadedFileCommandHandler _handler;

        public VoidUploadedFileCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repositoryMock = new Mock<IGenericRepository<UploadedFile>>();
            _fileStorageServiceMock = new Mock<Shared.Services.Contracts.IFileStorageService>();
            _cacheInvalidationServiceMock = new Mock<global::Persistence.Caching.Contracts.ICacheInvalidationService>();
            _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<VoidUploadedFileCommandHandler>>();

            _unitOfWorkMock.Setup(u => u.Repository<UploadedFile>())
                .Returns(_repositoryMock.Object);

            // Setup file storage service
            _fileStorageServiceMock.Setup(f => f.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Setup cache invalidation
            _cacheInvalidationServiceMock.Setup(c => c.InvalidateEntityListCacheAsync<UploadedFile>(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new VoidUploadedFileCommandHandler(
                _unitOfWorkMock.Object,
                _fileStorageServiceMock.Object,
                _cacheInvalidationServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingActiveFile_ShouldMarkAsInactive()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var existingFile = new UploadedFile
            {
                Id = fileId,
                Name = "test-document.pdf",
                Type = "Document",
                Extension = ".pdf",
                Size = 1.5m,
                Path = "C:\\Files\\test-document.pdf",
                Reference = "REF-001",
                Active = true
            };

            var command = new VoidUploadedFileCommand { Id = fileId.ToString() };

            #pragma warning disable CS8620 // Nullability mismatch in test mock setup
            _repositoryMock.Setup(r => r.GetFirstAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<UploadedFile, bool>>>(),
                It.IsAny<CancellationToken>())!)
                .ReturnsAsync(existingFile);
            #pragma warning restore CS8620

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().Be(fileId.ToString());
            existingFile.Active.Should().BeFalse();
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentFile_ShouldThrowNotFoundException()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var command = new VoidUploadedFileCommand { Id = fileId.ToString() };

            #pragma warning disable CS8620 // Nullability mismatch in test mock setup
            _repositoryMock.Setup(r => r.GetFirstAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<UploadedFile, bool>>>(),
                It.IsAny<CancellationToken>())!)
                .ReturnsAsync((UploadedFile?)null);
            #pragma warning restore CS8620

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _handler.Handle(command, CancellationToken.None));

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_InvalidGuidFormat_ShouldThrowFormatException()
        {
            // Arrange
            var command = new VoidUploadedFileCommand { Id = "invalid-guid" };

            // Act & Assert
            await Assert.ThrowsAsync<FormatException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UpdateFailure_ShouldThrowInternalServerError()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var existingFile = new UploadedFile
            {
                Id = fileId,
                Name = "test-document.pdf",
                Active = true
            };

            var command = new VoidUploadedFileCommand { Id = fileId.ToString() };

            #pragma warning disable CS8620 // Nullability mismatch in test mock setup
            _repositoryMock.Setup(r => r.GetFirstAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<UploadedFile, bool>>>(),
                It.IsAny<CancellationToken>())!)
                .ReturnsAsync(existingFile);
            #pragma warning restore CS8620

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database update failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InternalServerError>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SaveChangesFailure_ShouldThrowInternalServerError()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var existingFile = new UploadedFile
            {
                Id = fileId,
                Name = "test-document.pdf",
                Active = true
            };

            var command = new VoidUploadedFileCommand { Id = fileId.ToString() };

            #pragma warning disable CS8620 // Nullability mismatch in test mock setup
            _repositoryMock.Setup(r => r.GetFirstAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<UploadedFile, bool>>>(),
                It.IsAny<CancellationToken>())!)
                .ReturnsAsync(existingFile);
            #pragma warning restore CS8620

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InternalServerError>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SuccessfulVoid_ShouldReturnCorrectMessage()
        {
            // Arrange
            var fileId = Guid.NewGuid();
            var fileName = "important-document.pdf";
            var existingFile = new UploadedFile
            {
                Id = fileId,
                Name = fileName,
                Active = true
            };

            var command = new VoidUploadedFileCommand { Id = fileId.ToString() };

            #pragma warning disable CS8620 // Nullability mismatch in test mock setup
            _repositoryMock.Setup(r => r.GetFirstAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<UploadedFile, bool>>>(),
                It.IsAny<CancellationToken>())!)
                .ReturnsAsync(existingFile);
            #pragma warning restore CS8620

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.FriendlyMessage.Should().Contain("UploadedFile");
            result.FriendlyMessage.Should().Contain(fileName);
            result.Total.Should().Be(1);
        }

        [Fact]
        public async Task Handle_AlreadyInactiveFile_ShouldStillMarkAsInactive()
        {
            // Arrange - File already inactive
            var fileId = Guid.NewGuid();
            var existingFile = new UploadedFile
            {
                Id = fileId,
                Name = "old-document.pdf",
                Active = false // Already inactive
            };

            var command = new VoidUploadedFileCommand { Id = fileId.ToString() };

            #pragma warning disable CS8620 // Nullability mismatch in test mock setup
            _repositoryMock.Setup(r => r.GetFirstAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<UploadedFile, bool>>>(),
                It.IsAny<CancellationToken>())!)
                .ReturnsAsync(existingFile);
            #pragma warning restore CS8620

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            existingFile.Active.Should().BeFalse();
        }
    }
}

