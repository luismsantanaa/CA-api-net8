using Application.Features.Utilities.UploadFiles.Comands.Create;
using Domain.Entities.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Persistence.Repositories.Contracts;
using Shared.Exceptions;
using System.Text;
using Xunit;

namespace Tests.Application.Utilities
{
    public class UploadFileCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IGenericRepository<UploadedFile>> _repositoryMock;
        private readonly Mock<Shared.Services.Contracts.IFileStorageService> _fileStorageServiceMock;
        private readonly Mock<global::Persistence.Caching.Contracts.ICacheInvalidationService> _cacheInvalidationServiceMock;
        private readonly Mock<Microsoft.Extensions.Logging.ILogger<UploadFileCommandHandler>> _loggerMock;
        private readonly UploadFileCommandHandler _handler;

        public UploadFileCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configurationMock = new Mock<IConfiguration>();
            _repositoryMock = new Mock<IGenericRepository<UploadedFile>>();
            _fileStorageServiceMock = new Mock<Shared.Services.Contracts.IFileStorageService>();
            _cacheInvalidationServiceMock = new Mock<global::Persistence.Caching.Contracts.ICacheInvalidationService>();
            _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<UploadFileCommandHandler>>();

            // Setup configuration
            _configurationMock.Setup(c => c["FilesPaths:TestPath"])
                .Returns("C:\\TestFiles");

            // Setup repository
            _unitOfWorkMock.Setup(u => u.Repository<UploadedFile>())
                .Returns(_repositoryMock.Object);

            // Setup file storage service
            _fileStorageServiceMock.Setup(f => f.EnsureDirectoryExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _fileStorageServiceMock.Setup(f => f.SaveFileAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((IFormFile file, string dir, string? name, CancellationToken ct) =>
                    System.IO.Path.Combine(dir, file.FileName));

            // Setup cache invalidation
            _cacheInvalidationServiceMock.Setup(c => c.InvalidateEntityListCacheAsync<UploadedFile>(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Setup UnitOfWork transaction methods
            _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _handler = new UploadFileCommandHandler(
                _unitOfWorkMock.Object,
                _configurationMock.Object,
                _fileStorageServiceMock.Object,
                _cacheInvalidationServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ValidSingleFile_ShouldReturnSuccess()
        {
            // Arrange
            var command = new UploadFileCommand
            {
                Files = new List<IFormFile> { CreateMockFile("test.pdf", 1000000) },
                Type = "Document",
                Reference = "REF-001",
                Comment = "Test upload"
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Succeeded.Should().BeTrue();
            result.Items.Should().BeTrue();
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_MultipleValidFiles_ShouldSaveAll()
        {
            // Arrange
            var command = new UploadFileCommand
            {
                Files = new List<IFormFile>
                {
                    CreateMockFile("file1.pdf", 500000),
                    CreateMockFile("file2.docx", 800000),
                    CreateMockFile("file3.xlsx", 600000)
                },
                Type = "Documents",
                Reference = "REF-002"
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        public async Task Handle_FileWithInvalidExtension_ShouldThrowException()
        {
            // Arrange
            var command = new UploadFileCommand
            {
                Files = new List<IFormFile> { CreateMockFile("malware.exe", 1000) },
                Type = "Document",
                Reference = "REF-003"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_FileTooLarge_ShouldThrowException()
        {
            // Arrange - File larger than 3MB
            var command = new UploadFileCommand
            {
                Files = new List<IFormFile> { CreateMockFile("large.pdf", 4000000) },
                Type = "Document",
                Reference = "REF-004"
            };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmptyFilesList_ShouldThrowException()
        {
            // Arrange
            var command = new UploadFileCommand
            {
                Files = null,
                Type = "Document",
                Reference = "REF-005"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InternalServerError>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SaveChangesFailure_ShouldThrowInternalServerError()
        {
            // Arrange
            var command = new UploadFileCommand
            {
                Files = new List<IFormFile> { CreateMockFile("test.pdf", 1000000) },
                Type = "Document",
                Reference = "REF-006"
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<InternalServerError>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Theory]
        [InlineData("document.pdf")]
        [InlineData("spreadsheet.xlsx")]
        [InlineData("image.png")]
        [InlineData("presentation.pptx")]
        public async Task Handle_DifferentValidExtensions_ShouldSucceed(string fileName)
        {
            // Arrange
            var command = new UploadFileCommand
            {
                Files = new List<IFormFile> { CreateMockFile(fileName, 500000) },
                Type = "Document",
                Reference = "REF-007"
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidFile_ShouldSetCorrectMetadata()
        {
            // Arrange
            var fileName = "test-document.pdf";
            var fileSize = 1500000L; // ~1.43 MB
            UploadedFile? capturedFile = null;

            var command = new UploadFileCommand
            {
                Files = new List<IFormFile> { CreateMockFile(fileName, fileSize) },
                Type = "Invoice",
                Reference = "INV-2024-001",
                Comment = "Q1 Invoice"
            };

            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<UploadedFile>(), It.IsAny<CancellationToken>()))
                .Callback<UploadedFile, CancellationToken>((file, _) => capturedFile = file)
                .ReturnsAsync((UploadedFile file, CancellationToken ct) => file);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            capturedFile.Should().NotBeNull();
            capturedFile!.Name.Should().Be(fileName);
            capturedFile.Type.Should().Be("Invoice");
            capturedFile.Reference.Should().Be("INV-2024-001");
            capturedFile.Comment.Should().Be("Q1 Invoice");
            capturedFile.Extension.Should().Be(".pdf");
            capturedFile.Size.Should().BeApproximately(1.43m, 0.01m); // ~1.43 MB
        }

        // Helper method to create mock IFormFile
        private static IFormFile CreateMockFile(string fileName, long size)
        {
            var fileMock = new Mock<IFormFile>();
            var content = Encoding.UTF8.GetBytes(new string('*', (int)size));
            var ms = new MemoryStream(content);

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(size);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) =>
                {
                    ms.Position = 0;
                    return ms.CopyToAsync(stream, token);
                });

            return fileMock.Object;
        }
    }
}

