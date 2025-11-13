using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Security.Entities.DTOs;

namespace Tests.Helpers
{
    /// <summary>
    /// Helper class to create test fixtures and mocks for testing.
    /// </summary>
    public static class TestFixture
    {
        /// <summary>
        /// Creates a mock IOptions<JwtSettings> for testing JWT utilities.
        /// </summary>
        public static IOptions<JwtSettings> CreateJwtSettings(string key = "TestJwtKey123456789012345678901234567890",
            string issuer = "test.issuer",
            string audience = "test.audience",
            int durationInMinutes = 60)
        {
            var jwtSettings = new JwtSettings
            {
                Key = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key)),
                Issuer = issuer,
                Audience = audience,
                DurationInMinutes = durationInMinutes,
                ExpireTime = TimeSpan.FromMinutes(durationInMinutes) // Set ExpireTime
            };

            return Options.Create(jwtSettings);
        }

        /// <summary>
        /// Creates a mock ILogger<T> for testing.
        /// </summary>
        public static ILogger<T> CreateLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }

        /// <summary>
        /// Creates a test user ID as Guid.
        /// </summary>
        public static Guid CreateTestUserId()
        {
            return Guid.Parse("e6f819a9-1ca8-455e-87d7-97a4e4b7e042");
        }

        /// <summary>
        /// Creates a test email for testing.
        /// </summary>
        public static string CreateTestEmail()
        {
            return "testuser@test.com";
        }
    }
}

