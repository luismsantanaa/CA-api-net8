using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppApi.Authorization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Security.Entities.DTOs;
using Tests.Helpers;

namespace Tests.Authorization
{
    /// <summary>
    /// Unit tests for JwtUtils class.
    /// Tests JWT token validation, claim extraction, and error handling.
    /// </summary>
    public class JwtUtilsTests
    {
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly ILogger<JwtUtils> _logger;

        public JwtUtilsTests()
        {
            _jwtSettings = TestFixture.CreateJwtSettings();
            _logger = TestFixture.CreateLogger<JwtUtils>();
        }

        [Fact]
        public void ValidateToken_WithValidToken_ShouldReturnUserId()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);
            var userId = TestFixture.CreateTestUserId();
            var token = GenerateTestToken(userId.ToString(), _jwtSettings.Value);

            // Act
            var result = jwtUtils.ValidateToken(token);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(userId.ToString());
        }

        [Fact]
        public void ValidateToken_WithNullToken_ShouldReturnNull()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);

            // Act
            var result = jwtUtils.ValidateToken(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_WithEmptyToken_ShouldReturnNull()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);

            // Act
            var result = jwtUtils.ValidateToken(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ShouldReturnNull()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);
            var invalidToken = "invalid.jwt.token";

            // Act
            var result = jwtUtils.ValidateToken(invalidToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_WithExpiredToken_ShouldReturnNull()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);
            var expiredToken = GenerateExpiredToken(TestFixture.CreateTestUserId().ToString(), _jwtSettings.Value);

            // Act
            var result = jwtUtils.ValidateToken(expiredToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_WithTokenMissingUidClaim_ShouldReturnNull()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);
            var tokenWithoutUid = GenerateTokenWithoutUidClaim(_jwtSettings.Value);

            // Act
            var result = jwtUtils.ValidateToken(tokenWithoutUid);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetClaim_WithValidToken_ShouldReturnClaimValue()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);
            var userId = TestFixture.CreateTestUserId();
            var token = GenerateTestToken(userId.ToString(), _jwtSettings.Value);

            // Act
            var result = jwtUtils.GetClaim(token, "uid");

            // Assert
            result.Should().Be(userId.ToString());
        }

        [Fact]
        public void GetClaim_WithBearerPrefix_ShouldReturnClaimValue()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);
            var userId = TestFixture.CreateTestUserId();
            var token = $"Bearer {GenerateTestToken(userId.ToString(), _jwtSettings.Value)}";

            // Act
            var result = jwtUtils.GetClaim(token, "uid");

            // Assert
            result.Should().Be(userId.ToString());
        }

        [Fact]
        public void GetCurrentToken_WithValidToken_ShouldReturnClaimsList()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);
            var userId = TestFixture.CreateTestUserId();
            var token = GenerateTestToken(userId.ToString(), _jwtSettings.Value);

            // Act
            var result = jwtUtils.GetCurrentToken(token);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain(c => c.Type == "uid" && c.Value == userId.ToString());
        }

        [Fact]
        public void GetCurrentToken_WithNullToken_ShouldReturnNull()
        {
            // Arrange
            var jwtUtils = new JwtUtils(_jwtSettings, _logger);

            // Act
            var result = jwtUtils.GetCurrentToken(null!);

            // Assert
            result.Should().BeNull();
        }

        private string GenerateTestToken(string userId, JwtSettings settings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.Key);

            var claims = new List<Claim>
            {
                new Claim("uid", userId),
                new Claim(ClaimTypes.Email, TestFixture.CreateTestEmail()),
                new Claim(JwtRegisteredClaimNames.Sub, "testuser"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(settings.DurationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            if (!string.IsNullOrWhiteSpace(settings.Issuer))
            {
                tokenDescriptor.Issuer = settings.Issuer;
            }

            if (!string.IsNullOrWhiteSpace(settings.Audience))
            {
                tokenDescriptor.Audience = settings.Audience;
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateExpiredToken(string userId, JwtSettings settings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.Key);

            var claims = new List<Claim>
            {
                new Claim("uid", userId)
            };

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = now.AddMinutes(-30), // Token valid from 30 minutes ago (must be before Expires)
                Expires = now.AddMinutes(-10), // Expired 10 minutes ago (must be after NotBefore)
                IssuedAt = now.AddMinutes(-25), // Issued 25 minutes ago
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateTokenWithoutUidClaim(JwtSettings settings)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(settings.Key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, TestFixture.CreateTestEmail())
                // No uid claim
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

