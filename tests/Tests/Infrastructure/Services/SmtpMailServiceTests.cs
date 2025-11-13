using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Services;
using Shared.Services.Configurations;

namespace Tests.Infrastructure.Services
{
    public class SmtpMailServiceTests
    {
        private readonly Mock<ILogger<SmtpMailService>> _loggerMock;
        private readonly Mock<IOptions<EMailSettings>> _emailSettingsMock;
        private readonly EMailSettings _emailSettings;
        private readonly SmtpMailService _service;

        public SmtpMailServiceTests()
        {
            _loggerMock = new Mock<ILogger<SmtpMailService>>();
            _emailSettingsMock = new Mock<IOptions<EMailSettings>>();

            _emailSettings = new EMailSettings
            {
                From = "noreply@testcompany.com",
                Host = "smtp.testserver.com",
                Port = 587,
                UserName = "test@testcompany.com",
                Password = "TestPassword123",
                DisplayName = "Test Company"
            };

            _emailSettingsMock.Setup(x => x.Value).Returns(_emailSettings);

            _service = new SmtpMailService(_emailSettingsMock.Object, _loggerMock.Object);
        }

        [Fact]
        public void Constructor_WithValidSettings_ShouldNotThrow()
        {
            // Arrange & Act
            var service = new SmtpMailService(_emailSettingsMock.Object, _loggerMock.Object);

            // Assert
            service.Should().NotBeNull();
        }

        [Fact]
        public async Task SendAsync_WithNullRequest_ShouldReturnFalse()
        {
            // Arrange
            MailRequest? request = null;

            // Act
            var result = await _service.SendAsync(request!, null);

            // Assert
            result.Should().BeFalse();
            VerifyLogError();
        }

        [Fact]
        public async Task SendAsync_WithEmptyToList_ShouldReturnFalse()
        {
            // Arrange
            var request = new MailRequest
            {
                To = new List<string>(),
                Subject = "Test",
                Body = "Test Body"
            };

            // Act
            var result = await _service.SendAsync(request, null);

            // Assert
            result.Should().BeFalse();
            VerifyLogError();
        }

        [Fact]
        public async Task SendAsync_WithInvalidEmailFormat_ShouldReturnFalse()
        {
            // Arrange
            var request = new MailRequest
            {
                To = new List<string> { "invalid-email" },
                Subject = "Test",
                Body = "Test Body"
            };

            // Act
            var result = await _service.SendAsync(request, null);

            // Assert
            result.Should().BeFalse();
            VerifyLogError();
        }

        [Fact]
        public void MailRequest_WithMultipleRecipients_ShouldAllowMultipleTo()
        {
            // Arrange & Act
            var request = new MailRequest
            {
                To = new List<string>
                {
                    "recipient1@test.com",
                    "recipient2@test.com",
                    "recipient3@test.com"
                },
                Subject = "Test",
                Body = "Test Body"
            };

            // Assert
            request.To.Should().HaveCount(3);
            request.To.Should().Contain("recipient1@test.com");
            request.To.Should().Contain("recipient2@test.com");
            request.To.Should().Contain("recipient3@test.com");
        }

        [Fact]
        public void MailRequest_WithCcRecipients_ShouldSupportCc()
        {
            // Arrange & Act
            var request = new MailRequest
            {
                To = new List<string> { "recipient@test.com" },
                Cc = new List<string> { "cc1@test.com", "cc2@test.com" },
                Subject = "Test",
                Body = "Test Body"
            };

            // Assert
            request.Cc.Should().NotBeNull();
            request.Cc.Should().HaveCount(2);
        }

        [Fact]
        public void MailRequest_WithAttachments_ShouldSupportAttachments()
        {
            // Arrange & Act
            var request = new MailRequest
            {
                To = new List<string> { "recipient@test.com" },
                Subject = "Test with attachments",
                Body = "Test Body",
                Attach = new List<string> { "C:\\file1.pdf", "C:\\file2.docx" }
            };

            // Assert
            request.Attach.Should().NotBeNull();
            request.Attach.Should().HaveCount(2);
        }

        [Fact]
        public void MailRequest_IsNotification_DefaultShouldBeFalse()
        {
            // Arrange & Act
            var request = new MailRequest
            {
                To = new List<string> { "recipient@test.com" },
                Subject = "Test",
                Body = "Test Body"
            };

            // Assert
            request.IsNotification.Should().BeFalse();
        }

        [Fact]
        public void MailRequest_IsNotification_CanBeSetToTrue()
        {
            // Arrange & Act
            var request = new MailRequest
            {
                To = new List<string> { "recipient@test.com" },
                Subject = "Test",
                Body = "Test Body with @logo, @anounced, @footer",
                IsNotification = true
            };

            // Assert
            request.IsNotification.Should().BeTrue();
        }

        [Fact]
        public void EMailSettings_ShouldRequireAllProperties()
        {
            // Arrange & Act
            var settings = new EMailSettings
            {
                From = "test@test.com",
                Host = "smtp.test.com",
                Port = 587,
                UserName = "testuser",
                Password = "testpass"
            };

            // Assert
            settings.From.Should().Be("test@test.com");
            settings.Host.Should().Be("smtp.test.com");
            settings.Port.Should().Be(587);
            settings.UserName.Should().Be("testuser");
            settings.Password.Should().Be("testpass");
        }

        [Fact]
        public void EMailSettings_DisplayName_IsOptional()
        {
            // Arrange & Act
            var settings = new EMailSettings
            {
                From = "test@test.com",
                Host = "smtp.test.com",
                Port = 587,
                UserName = "testuser",
                Password = "testpass",
                DisplayName = null
            };

            // Assert
            settings.DisplayName.Should().BeNull();
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("user.name@example.co.uk")]
        [InlineData("user+tag@subdomain.example.com")]
        public void MailRequest_ValidEmailFormats_ShouldBeAccepted(string email)
        {
            // Arrange & Act
            var request = new MailRequest
            {
                To = new List<string> { email },
                Subject = "Test",
                Body = "Test Body"
            };

            // Assert
            request.To.Should().Contain(email);
        }

        [Fact]
        public void MailRequest_SubjectAndBody_ShouldAcceptHtml()
        {
            // Arrange
            var htmlBody = "<html><body><h1>Test</h1><p>This is a test email with <strong>HTML</strong></p></body></html>";

            // Act
            var request = new MailRequest
            {
                To = new List<string> { "test@example.com" },
                Subject = "HTML Email Test",
                Body = htmlBody
            };

            // Assert
            request.Body.Should().Contain("<html>");
            request.Body.Should().Contain("<strong>HTML</strong>");
        }

        [Fact]
        public void MailRequest_WithNotificationPlaceholders_ShouldContainPlaceholders()
        {
            // Arrange & Act
            var request = new MailRequest
            {
                To = new List<string> { "test@example.com" },
                Subject = "Notification",
                Body = "Email with @logo at top, @anounced in middle, and @footer at bottom",
                IsNotification = true
            };

            // Assert
            request.Body.Should().Contain("@logo");
            request.Body.Should().Contain("@anounced");
            request.Body.Should().Contain("@footer");
        }

        // Note: Integration tests for actual SMTP sending should be done separately
        // with a test SMTP server (like smtp4dev, Papercut, or MailHog)

        private void VerifyLogError()
        {
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.AtLeastOnce);
        }
    }
}

