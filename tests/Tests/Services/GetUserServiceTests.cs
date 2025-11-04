using AppApi.Authorization;
using AppApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for GetUserService class.
    /// Tests user ID extraction from JWT tokens and authentication status.
    /// </summary>
    public class GetUserServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IJwtUtils> _jwtUtilsMock;
        private readonly ILogger<GetUserService> _logger;

        public GetUserServiceTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _jwtUtilsMock = new Mock<IJwtUtils>();
            _logger = TestFixture.CreateLogger<GetUserService>();
        }

        [Fact]
        public void UserId_WithValidToken_ShouldReturnUserId()
        {
            // Arrange
            var userId = TestFixture.CreateTestUserId();
            var token = "Bearer valid.token.here";
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = token;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            _jwtUtilsMock.Setup(x => x.GetClaim(token, "uid")).Returns(userId.ToString());

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.UserId;

            // Assert
            result.Should().Be(userId);
        }

        [Fact]
        public void UserId_WithNullHttpContext_ShouldReturnNull()
        {
            // Arrange
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.UserId;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void UserId_WithMissingAuthorizationHeader_ShouldReturnNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            // No Authorization header
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.UserId;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void UserId_WithInvalidUserIdFormat_ShouldReturnNull()
        {
            // Arrange
            var token = "Bearer valid.token.here";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = token;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            _jwtUtilsMock.Setup(x => x.GetClaim(token, "uid")).Returns("invalid-guid-format");

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.UserId;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void IsAuthenticated_WithValidToken_ShouldReturnTrue()
        {
            // Arrange
            var userId = TestFixture.CreateTestUserId();
            var token = "Bearer valid.token.here";
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = token;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            _jwtUtilsMock.Setup(x => x.GetClaim(token, "uid")).Returns(userId.ToString());

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.IsAuthenticated;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsAuthenticated_WithNullHttpContext_ShouldReturnFalse()
        {
            // Arrange
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.IsAuthenticated;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsAuthenticated_WithMissingAuthorizationHeader_ShouldReturnFalse()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.IsAuthenticated;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void UserId_WithExceptionInJwtUtils_ShouldReturnNull()
        {
            // Arrange
            var token = "Bearer valid.token.here";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = token;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            _jwtUtilsMock.Setup(x => x.GetClaim(token, "uid")).Throws(new Exception("JWT parsing error"));

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act
            var result = service.UserId;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void UserId_ShouldUseLazyInitialization_OnlyCallJwtUtilsOnce()
        {
            // Arrange
            var userId = TestFixture.CreateTestUserId();
            var token = "Bearer valid.token.here";
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = token;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
            
            _jwtUtilsMock.Setup(x => x.GetClaim(token, "uid")).Returns(userId.ToString());

            var service = new GetUserService(_httpContextAccessorMock.Object, _jwtUtilsMock.Object, _logger);

            // Act - Access UserId multiple times
            var result1 = service.UserId;
            var result2 = service.UserId;
            var result3 = service.UserId;

            // Assert
            result1.Should().Be(userId);
            result2.Should().Be(userId);
            result3.Should().Be(userId);
            _jwtUtilsMock.Verify(x => x.GetClaim(token, "uid"), Times.Once);
        }
    }
}

