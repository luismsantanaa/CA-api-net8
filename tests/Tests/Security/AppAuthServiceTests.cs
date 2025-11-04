using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Security.DbContext;
using Security.Entities;
using Security.Entities.DTOs;
using Security.Services.Concrete;
using Security.Services.Contracts;
using Shared.Exceptions;
using Shared.Extensions.Contracts;
using Shared.Services.Contracts;
using System.Text;
using Tests.Helpers;

namespace Tests.Security
{
    /// <summary>
    /// Unit tests for AppAuthService.
    /// Tests authentication scenarios that reflect real production use cases:
    /// - Successful login with Identity password
    /// - Successful login with Active Directory
    /// - Failed login scenarios (user not found, wrong password)
    /// - Token generation and refresh token storage
    /// - Refresh token scenarios (valid, expired, already used, revoked)
    /// </summary>
    public class AppAuthServiceTests : IDisposable
    {
        private readonly IdentityTestHelper _identityHelper;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly Mock<IActiveDirectoryService> _adServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IGenericHttpClient> _httpClientMock;
        private readonly Mock<ILocalTimeService> _localTimeServiceMock;
        private readonly Mock<IThrowException> _exceptionMock;
        private readonly Mock<IJsonService> _jsonServiceMock;
        private readonly Mock<SignInManager<IdentityUser>> _signInManagerMock;

        public AppAuthServiceTests()
        {
            _identityHelper = new IdentityTestHelper();
            
            _jwtSettings = TestFixture.CreateJwtSettings();
            
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Value.Key);
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                RequireExpirationTime = false,
                ClockSkew = TimeSpan.Zero,
            };

            _adServiceMock = new Mock<IActiveDirectoryService>();
            _configurationMock = new Mock<IConfiguration>();
            _httpClientMock = new Mock<IGenericHttpClient>();
            _localTimeServiceMock = new Mock<ILocalTimeService>();
            _localTimeServiceMock.Setup(x => x.LocalTime).Returns(DateTime.UtcNow);
            _exceptionMock = new Mock<IThrowException>();
            _jsonServiceMock = new Mock<IJsonService>();
            
            // Mock SignInManager to avoid HttpContext issues in tests
            _signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                _identityHelper.UserManager,
                Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                Options.Create(new IdentityOptions()),
                Mock.Of<ILogger<SignInManager<IdentityUser>>>(),
                Mock.Of<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<IdentityUser>>());
        }

        [Fact]
        public async Task UserAuthentication_WithValidIdentityUser_ShouldReturnSuccessWithTokens()
        {
            // Arrange - Create a test user in database (simulates real production scenario)
            var testUser = await _identityHelper.CreateTestUserAsync(
                email: "testuser@test.com",
                userName: "testuser",
                password: "TestPassword123!");

            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false); // User not in AD

            var service = CreateAppAuthService();

            var request = new AuthRequest
            {
                Email = "testuser@test.com",
                Password = "TestPassword123!"
            };

            // Act
            var result = await service.UserAuthentication(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Email.Should().Be(testUser.Email);
            result.Username.Should().Be(testUser.UserName);
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.Id.Should().Be(testUser.Id);

            // Verify refresh token was saved to database (real production behavior)
            var savedRefreshToken = await _identityHelper.Context.RefreshTokens!
                .FirstOrDefaultAsync(x => x.Token == result.RefreshToken);
            savedRefreshToken.Should().NotBeNull();
            savedRefreshToken!.UserId.Should().Be(testUser.Id);
            savedRefreshToken.IsUsed.Should().BeFalse();
            savedRefreshToken.IsRevoked.Should().BeFalse();
        }

        [Fact]
        public async Task UserAuthentication_WithActiveDirectoryUser_ShouldReturnSuccessWithTokens()
        {
            // Arrange - Create a test user in database
            var testUser = await _identityHelper.CreateTestUserAsync(
                email: "aduser@test.com",
                userName: "aduser",
                password: "ADPassword123!");

            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(true); // User exists in AD
            _adServiceMock.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true); // AD auth succeeds

            var service = CreateAppAuthService();

            var request = new AuthRequest
            {
                Email = "aduser@test.com",
                Password = "ADPassword123!"
            };

            // Act
            var result = await service.UserAuthentication(request);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();

            // Verify AD authentication was called
            _adServiceMock.Verify(x => x.Authenticate("aduser@test.com", "ADPassword123!"), Times.Once);
        }

        [Fact]
        public async Task UserAuthentication_WithNonExistentUser_ShouldThrowException()
        {
            // Arrange
            // The service uses _exception.IfNull which throws ArgumentNullException via extension method
            // We'll let the actual extension method execute, but we need to mock the behavior
            // Since IfNull is an extension method, we can't easily mock it
            // Instead, we test the actual behavior - when user is null, FindByEmailAsync returns null
            // and the service throws via the exception extension

            var service = CreateAppAuthService();

            var request = new AuthRequest
            {
                Email = "nonexistent@test.com",
                Password = "AnyPassword123!"
            };

            // Act & Assert - The service will throw ArgumentNullException when user is null
            // The actual implementation uses ThrowException.Exception.IfNull which throws ArgumentNullException
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await service.UserAuthentication(request));
        }

        [Fact]
        public async Task UserAuthentication_WithWrongPassword_ShouldThrowException()
        {
            // Arrange - Create test user with one password
            var testUser = await _identityHelper.CreateTestUserAsync(
                email: "testuser@test.com",
                userName: "testuser",
                password: "CorrectPassword123!");

            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false);

            // Use service with failed sign-in mock
            var service = CreateAppAuthServiceWithFailedSignIn();

            var request = new AuthRequest
            {
                Email = "testuser@test.com",
                Password = "WrongPassword123!" // Wrong password
            };

            // Act & Assert - Should throw exception for authentication failure
            // The service throws ArgumentException via ThrowException.Exception.IfFalse
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await service.UserAuthentication(request));
        }

        [Fact]
        public async Task UserAuthentication_WithActiveDirectoryAuthFailure_ShouldThrowException()
        {
            // Arrange - Create test user
            var testUser = await _identityHelper.CreateTestUserAsync(
                email: "aduser@test.com",
                userName: "aduser",
                password: "SomePassword123!");

            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(true); // User exists in AD
            _adServiceMock.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false); // AD auth fails

            var service = CreateAppAuthService();

            var request = new AuthRequest
            {
                Email = "aduser@test.com",
                Password = "WrongPassword123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<AuthorizationValidationException>(
                async () => await service.UserAuthentication(request));
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
        {
            // Arrange - Create user and generate initial token
            var testUser = await _identityHelper.CreateTestUserAsync();
            var service = CreateAppAuthService();
            
            var loginRequest = new AuthRequest
            {
                Email = testUser.Email!,
                Password = "TestPassword123!"
            };
            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false);
            
            var loginResult = await service.UserAuthentication(loginRequest);
            
            // Get the JTI from the token
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);
            var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;

            var refreshRequest = new TokenRequest
            {
                Token = loginResult.Token,
                RefreshToken = loginResult.RefreshToken
            };

            // Act
            var result = await service.RefreshToken(refreshRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.Token.Should().NotBe(loginResult.Token); // Should be a new token
            result.RefreshToken.Should().NotBe(loginResult.RefreshToken); // Should be a new refresh token

            // Verify old refresh token was marked as used
            var oldToken = await _identityHelper.Context.RefreshTokens!
                .FirstOrDefaultAsync(x => x.Token == loginResult.RefreshToken);
            oldToken.Should().NotBeNull();
            oldToken!.IsUsed.Should().BeTrue();

            // Verify new refresh token was saved
            var newToken = await _identityHelper.Context.RefreshTokens!
                .FirstOrDefaultAsync(x => x.Token == result.RefreshToken);
            newToken.Should().NotBeNull();
            newToken!.IsUsed.Should().BeFalse();
        }

        [Fact]
        public async Task RefreshToken_WithExpiredRefreshToken_ShouldReturnError()
        {
            // Arrange - Create user and expired refresh token
            var testUser = await _identityHelper.CreateTestUserAsync();
            var service = CreateAppAuthService();
            
            // Generate a token
            var loginRequest = new AuthRequest
            {
                Email = testUser.Email!,
                Password = "TestPassword123!"
            };
            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false);
            
            var loginResult = await service.UserAuthentication(loginRequest);
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);
            var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value ?? Guid.NewGuid().ToString();

            // Create an expired refresh token in database
            var expiredToken = await _identityHelper.SeedRefreshTokenAsync(
                testUser.Id,
                jti,
                "expired_refresh_token",
                expireDate: DateTime.UtcNow.AddDays(-1)); // Expired yesterday

            var refreshRequest = new TokenRequest
            {
                Token = loginResult.Token,
                RefreshToken = expiredToken.Token
            };

            // Act
            var result = await service.RefreshToken(refreshRequest);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("El refresh token has expired!");
        }

        [Fact]
        public async Task RefreshToken_WithAlreadyUsedToken_ShouldReturnError()
        {
            // Arrange - Create user and used refresh token
            var testUser = await _identityHelper.CreateTestUserAsync();
            var service = CreateAppAuthService();
            
            var loginRequest = new AuthRequest
            {
                Email = testUser.Email!,
                Password = "TestPassword123!"
            };
            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false);
            
            var loginResult = await service.UserAuthentication(loginRequest);
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);
            var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value ?? Guid.NewGuid().ToString();

            // Create an already used refresh token
            var usedToken = await _identityHelper.SeedRefreshTokenAsync(
                testUser.Id,
                jti,
                "used_refresh_token",
                isUsed: true);

            var refreshRequest = new TokenRequest
            {
                Token = loginResult.Token,
                RefreshToken = usedToken.Token
            };

            // Act
            var result = await service.RefreshToken(refreshRequest);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("The token has already been used");
        }

        [Fact]
        public async Task RefreshToken_WithRevokedToken_ShouldReturnError()
        {
            // Arrange - Create user and revoked refresh token
            var testUser = await _identityHelper.CreateTestUserAsync();
            var service = CreateAppAuthService();
            
            var loginRequest = new AuthRequest
            {
                Email = testUser.Email!,
                Password = "TestPassword123!"
            };
            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false);
            
            var loginResult = await service.UserAuthentication(loginRequest);
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(loginResult.Token);
            var jti = jwtToken.Claims.FirstOrDefault(x => x.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value ?? Guid.NewGuid().ToString();

            // Create a revoked refresh token
            var revokedToken = await _identityHelper.SeedRefreshTokenAsync(
                testUser.Id,
                jti,
                "revoked_refresh_token",
                isRevoked: true);

            var refreshRequest = new TokenRequest
            {
                Token = loginResult.Token,
                RefreshToken = revokedToken.Token
            };

            // Act
            var result = await service.RefreshToken(refreshRequest);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("The token has been revoked");
        }

        [Fact]
        public async Task RefreshToken_WithNonExistentRefreshToken_ShouldReturnError()
        {
            // Arrange - Create user and token
            var testUser = await _identityHelper.CreateTestUserAsync();
            var service = CreateAppAuthService();
            
            var loginRequest = new AuthRequest
            {
                Email = testUser.Email!,
                Password = "TestPassword123!"
            };
            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false);
            
            var loginResult = await service.UserAuthentication(loginRequest);

            var refreshRequest = new TokenRequest
            {
                Token = loginResult.Token,
                RefreshToken = "nonexistent_refresh_token_12345" // Token that doesn't exist in DB
            };

            // Act
            var result = await service.RefreshToken(refreshRequest);

            // Assert
            result.Success.Should().BeFalse();
            result.Errors.Should().Contain("El Token no existe");
        }

        [Fact]
        public async Task UserAuthentication_GeneratedToken_ShouldContainCorrectClaims()
        {
            // Arrange
            var testUser = await _identityHelper.CreateTestUserAsync();
            _adServiceMock.Setup(x => x.UserExist(It.IsAny<string>())).ReturnsAsync(false);

            var service = CreateAppAuthService();

            var request = new AuthRequest
            {
                Email = testUser.Email!,
                Password = "TestPassword123!"
            };

            // Act
            var result = await service.UserAuthentication(request);

            // Assert
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(result.Token);

            // Verify token contains required claims (production requirements)
            jwtToken.Claims.Should().Contain(c => c.Type == "uid" && c.Value == testUser.Id);
            // JWT tokens use JwtRegisteredClaimNames.Email, not ClaimTypes.Email
            jwtToken.Claims.Should().Contain(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email && c.Value == testUser.Email);
            jwtToken.Claims.Should().Contain(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub && c.Value == testUser.UserName);
            jwtToken.Claims.Should().Contain(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti);
            jwtToken.Claims.Should().Contain(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp);
        }

        private AppAuthService CreateAppAuthService()
        {
            // Setup SignInManager mock to return successful sign-in for tests
            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            return new AppAuthService(
                _identityHelper.UserManager,
                _signInManagerMock.Object, // Use mocked SignInManager
                _jwtSettings,
                _identityHelper.Context,
                _tokenValidationParameters,
                _adServiceMock.Object,
                _configurationMock.Object,
                _httpClientMock.Object,
                _localTimeServiceMock.Object,
                _exceptionMock.Object,
                _jsonServiceMock.Object);
        }
        
        private AppAuthService CreateAppAuthServiceWithFailedSignIn()
        {
            // Setup SignInManager mock to return failed sign-in
            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            return new AppAuthService(
                _identityHelper.UserManager,
                _signInManagerMock.Object,
                _jwtSettings,
                _identityHelper.Context,
                _tokenValidationParameters,
                _adServiceMock.Object,
                _configurationMock.Object,
                _httpClientMock.Object,
                _localTimeServiceMock.Object,
                _exceptionMock.Object,
                _jsonServiceMock.Object);
        }

        public void Dispose()
        {
            _identityHelper?.Dispose();
        }
    }
}

