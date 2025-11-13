using AppApi.Authorization;
using Microsoft.Extensions.Logging;
using Persistence.DbContexts.Contracts;

namespace AppApi.Services;

/// <summary>
/// Service for retrieving the current authenticated user information from JWT tokens.
/// Uses lazy evaluation to extract user information from the HTTP context when accessed.
/// </summary>
public class GetUserService : IGetUserServices
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJwtUtils _jwtUtils;
    private readonly ILogger<GetUserService>? _logger;

    /// <summary>
    /// Claim type for user identifier in JWT tokens.
    /// Should match CustomClaimTypes.Uid used when generating tokens.
    /// </summary>
    private const string UserClaimType = "uid";

    private Guid? _userId;
    private bool? _isAuthenticated;
    private bool _initialized = false;
    private readonly object _lock = new object();

    /// <summary>
    /// Initializes a new instance of GetUserService.
    /// User information is retrieved lazily when properties are first accessed.
    /// </summary>
    /// <param name="httpContextAccessor">Provides access to the HTTP context</param>
    /// <param name="jwtUtils">JWT utility service for token parsing</param>
    /// <param name="logger">Optional logger for error tracking</param>
    public GetUserService(IHttpContextAccessor httpContextAccessor, IJwtUtils jwtUtils, ILogger<GetUserService>? logger = null)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _jwtUtils = jwtUtils ?? throw new ArgumentNullException(nameof(jwtUtils));
        _logger = logger;
    }

    /// <summary>
    /// Gets the current user's ID from the JWT token.
    /// Extracted lazily on first access.
    /// </summary>
    public Guid? UserId
    {
        get
        {
            EnsureInitialized();
            return _userId;
        }
    }

    /// <summary>
    /// Gets whether the current user is authenticated.
    /// Determined lazily on first access.
    /// </summary>
    public bool? IsAuthenticated
    {
        get
        {
            EnsureInitialized();
            return _isAuthenticated;
        }
    }

    /// <summary>
    /// Ensures user information has been extracted from the JWT token.
    /// Uses double-checked locking pattern for thread safety.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        lock (_lock)
        {
            if (_initialized)
                return;

            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                
                if (httpContext == null)
                {
                    _logger?.LogDebug("HttpContext is null, user cannot be authenticated");
                    _userId = null;
                    _isAuthenticated = false;
                    _initialized = true;
                    return;
                }

                var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
                
                if (string.IsNullOrWhiteSpace(authHeader))
                {
                    _logger?.LogDebug("Authorization header is missing, user is not authenticated");
                    _userId = null;
                    _isAuthenticated = false;
                    _initialized = true;
                    return;
                }

                // Extract user ID from JWT token claim
                var uidValue = _jwtUtils.GetClaim(authHeader, UserClaimType);
                
                if (string.IsNullOrWhiteSpace(uidValue))
                {
                    _logger?.LogWarning("JWT token found but 'uid' claim is missing or empty");
                    _userId = null;
                    _isAuthenticated = false;
                }
                else if (Guid.TryParse(uidValue, out var parsedUserId))
                {
                    _userId = parsedUserId;
                    _isAuthenticated = true;
                    _logger?.LogDebug("User authenticated successfully. UserId: {UserId}", _userId);
                }
                else
                {
                    _logger?.LogWarning("JWT token contains invalid user ID format: {UidValue}", uidValue);
                    _userId = null;
                    _isAuthenticated = false;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error extracting user information from JWT token: {ErrorMessage}", ex.Message);
                _userId = null;
                _isAuthenticated = false;
            }
            finally
            {
                _initialized = true;
            }
        }
    }
}
