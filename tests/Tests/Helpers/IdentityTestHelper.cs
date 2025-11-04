using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Security.DbContext;
using Security.Entities;

namespace Tests.Helpers
{
    /// <summary>
    /// Helper class for creating Identity components for testing.
    /// Simplified approach that works with .NET 8 Identity architecture.
    /// </summary>
    public class IdentityTestHelper : IDisposable
    {
        public IdentityContext Context { get; }
        public UserManager<IdentityUser> UserManager { get; }
        public SignInManager<IdentityUser> SignInManager { get; }

        public IdentityTestHelper()
        {
            // Create InMemory database
            var options = new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(databaseName: $"TestIdentityDb_{Guid.NewGuid()}")
                .Options;

            Context = new IdentityContext(options);

            // Create UserManager using the context's store
            var store = new UserStore<IdentityUser>(Context);
            var identityOptions = Options.Create(new IdentityOptions());
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<UserManager<IdentityUser>>();

            UserManager = new UserManager<IdentityUser>(
                store,
                identityOptions,
                new PasswordHasher<IdentityUser>(),
                new List<IUserValidator<IdentityUser>>(),
                new List<IPasswordValidator<IdentityUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new ServiceCollection()
                    .AddLogging()
                    .BuildServiceProvider(),
                logger);

            // Create SignInManager with required dependencies for .NET 8
            // SignInManager requires an HttpContext with authentication services configured
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
            var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
            httpContextAccessor.HttpContext = httpContext; // Set the HttpContext
            
            var userOptions = Options.Create(new UserOptions());
            
            // In .NET 8, UserClaimsPrincipalFactory constructor signature may vary
            // Using the parameterless constructor approach or DI-created factory
            var claimsFactory = new UserClaimsPrincipalFactory<IdentityUser>(
                UserManager,
                identityOptions);
            
            var signInLogger = loggerFactory.CreateLogger<SignInManager<IdentityUser>>();
            var schemeProviderMock = new Moq.Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            var userConfirmation = new DefaultUserConfirmation<IdentityUser>();

            // SignInManager constructor in .NET 8
            SignInManager = new SignInManager<IdentityUser>(
                UserManager,
                httpContextAccessor,
                claimsFactory,
                identityOptions,
                signInLogger,
                schemeProviderMock.Object,
                userConfirmation);
        }

        /// <summary>
        /// Creates a test user with the specified credentials.
        /// </summary>
        public async Task<IdentityUser> CreateTestUserAsync(
            string email = "testuser@test.com",
            string userName = "testuser",
            string password = "TestPassword123!")
        {
            var user = new IdentityUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var result = await UserManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return user;
        }

        /// <summary>
        /// Seeds a RefreshToken in the database for testing refresh token scenarios.
        /// </summary>
        public async Task<RefreshToken> SeedRefreshTokenAsync(
            string userId,
            string jti,
            string refreshToken,
            bool isUsed = false,
            bool isRevoked = false,
            DateTime? expireDate = null)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                JwtId = jti,
                Token = refreshToken,
                IsUsed = isUsed,
                IsRevoked = isRevoked,
                CreatedDate = DateTime.UtcNow,
                ExpireDate = expireDate ?? DateTime.UtcNow.AddMonths(6)
            };

            Context.RefreshTokens!.Add(token);
            await Context.SaveChangesAsync();

            return token;
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}
