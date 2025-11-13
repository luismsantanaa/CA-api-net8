using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Security.Entities.DTOs;
using Shared.Exceptions;

namespace AppApi.Authorization
{
    public interface IJwtUtils
    {
        /// <summary>
        /// Validates a JWT token and returns the user ID claim value if valid.
        /// </summary>
        /// <param name="token">The JWT token string to validate</param>
        /// <returns>The user ID from the "uid" claim if token is valid, null otherwise</returns>
        public string? ValidateToken(string token);
        
        List<Claim>? GetCurrentToken(string jwt);
        string GetClaim(string token, string claimType);
    }

    public class JwtUtils : IJwtUtils
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtUtils>? _logger;

        /// <summary>
        /// Claim type for user identifier in JWT tokens.
        /// Should match CustomClaimTypes.Uid used when generating tokens.
        /// </summary>
        private const string UserIdClaimType = "uid";

        public JwtUtils(IOptions<JwtSettings> jwtSettings, ILogger<JwtUtils>? logger = null)
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Validates a JWT token and extracts the user ID from the "uid" claim.
        /// </summary>
        /// <param name="token">The JWT token string to validate</param>
        /// <returns>The user ID (string) from the "uid" claim if token is valid, null otherwise</returns>
        public string? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Key);
            
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(_jwtSettings.Issuer),
                    ValidateAudience = !string.IsNullOrWhiteSpace(_jwtSettings.Audience),
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Set Issuer and Audience only if they are configured
                if (!string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
                {
                    validationParameters.ValidIssuer = _jwtSettings.Issuer;
                }

                if (!string.IsNullOrWhiteSpace(_jwtSettings.Audience))
                {
                    validationParameters.ValidAudience = _jwtSettings.Audience;
                }

                tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                
                // Extract user ID from "uid" claim (matches CustomClaimTypes.Uid)
                var uidClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == UserIdClaimType);
                
                if (uidClaim == null || string.IsNullOrWhiteSpace(uidClaim.Value))
                {
                    _logger?.LogWarning("JWT token validated but 'uid' claim is missing or empty");
                    return null;
                }

                return uidClaim.Value;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger?.LogWarning(ex, "JWT token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger?.LogWarning(ex, "JWT token has invalid signature");
                return null;
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger?.LogWarning(ex, "JWT token validation failed: {Error}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unexpected error validating JWT token: {Error}", ex.Message);
                return null;
            }
        }

        public List<Claim>? GetCurrentToken(string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return null;

            jwt = jwt.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwt);

            return token.Claims.ToList();
        }

        public string GetClaim(string token, string claimType)
        {
            try
            {
                token = token.Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                return securityToken!.Claims.First(claim => claim.Type == claimType).Value;
            }
            catch (Exception ex)
            {
                string message = "";
                if (ex.InnerException == null)
                { message += ex.Message; }
                else { message += ex.InnerException.Message; }

                throw new InternalServerError(message, ex);
            }
        }
    }
}
