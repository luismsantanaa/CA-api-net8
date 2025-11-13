using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Security.DbContext;
using Security.Entities;
using Security.Entities.DTOs;
using Security.Repositories.Contracts;
using Security.Services.Contracts;
using Shared.Exceptions;
using Shared.Extensions;
using Shared.Extensions.Contracts;
using Shared.Services.Contracts;

namespace Security.Services.Concrete
{
    public class AppAuthService : IAppAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IdentityContext _context;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IActiveDirectoryService _adService;
        private readonly IConfiguration _configuration;
        private readonly IGenericHttpClient _httpClient;
        private readonly DateTime _localTime;
        private readonly IThrowException _exception;
        private readonly IJsonService _jsonService;
        private readonly IEmployeeRepository _employeeRepository;

        public AppAuthService(
           UserManager<IdentityUser> userManager,
           SignInManager<IdentityUser> signInManager,
           IOptions<JwtSettings> jwtSettings,
           IdentityContext identityDbContext,
           TokenValidationParameters tokenValidationParameters,
           IActiveDirectoryService adService,
           IConfiguration configuration,
           IGenericHttpClient httpClient,
           ILocalTimeService localTime,
           IThrowException exception,
           IJsonService jsonService,
           IEmployeeRepository employeeRepository
           )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _context = identityDbContext;
            _tokenValidationParameters = tokenValidationParameters;
            _adService = adService;
            _configuration = configuration;
            _httpClient = httpClient;
            _localTime = localTime.LocalTime;
            _exception = exception;
            _jsonService = jsonService;
            _employeeRepository = employeeRepository;
        }

        public async Task<AuthResponse> UserAuthentication(AuthRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            _exception.IfNull(user, nameof(user), $"No hay cuenta registrada en la aplicació para {request.Email}.");

            var adUserExist = await _adService.UserExist(request.Email);

            if (!adUserExist)
            {
                // login with identity user
                var signIn = await _signInManager.PasswordSignInAsync(user!.UserName!, request.Password, false, lockoutOnFailure: false);
                ThrowException.Exception.IfFalse(signIn.Succeeded, $"Ha ocurrido un error de autenticación!!");
            }
            else
            {
                // login with active directory
                var adUserIsValid = await _adService.Authenticate(request.Email, request.Password);
                if (!adUserIsValid) throw new AuthorizationValidationException(ErrorMessage.AuthorizationValidationException);
            }

            var token = await GenerateToken(user!);
            var authResponse = new AuthResponse
            {
                Id = user!.Id!,
                Token = token.Item1,
                Email = user.Email!,
                Username = user.UserName!,
                RefreshToken = token.Item2,
                Success = true,
            };

            return authResponse;
        }

        public async Task<RegistrationResponse> Register(RegistrationRequest request)
        {
            var existingUser = await _userManager.FindByNameAsync(request.UserName!);
            ThrowException.Exception.IfNotNull(existingUser, $"El usuario [{request.UserName}] ya esta registrado!");

            var existingEmail = await _userManager.FindByEmailAsync(request.Email!);
            ThrowException.Exception.IfNotNull(existingEmail, $"El Email [{request.Email}] ya esta registrado!");

            if (request.FullName == null && request.Department == null)
            {

                var service = new EmployeeService(request.UserName!, _employeeRepository);

                var employee = await service.GetEmployee()!;

                request.Codigo = employee!.Codigo!.Trim();
                request.FullName = employee.Nombre!.Trim();
                request.Email = employee.Email!.Trim();
                request.Department = employee.Departamento!.Trim();
                request.Position = employee.CargoNombre!.Trim();
                request.UserName = employee.UserName!.Trim();
                request.Password = employee.Nombre!.Trim();
                request.Office = employee.Oficina!.Trim();
                request.Password = null;
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                var password = string.Concat(char.ToUpper(request.UserName![0]), request.UserName![1..].ToLower());
                password = string.Concat(password, "@", _localTime.Year);

                request.Password = password;
            }

            var user = new IdentityUser
            {
                Email = request.Email,
                UserName = request.UserName,
                EmailConfirmed = true
            };

            var errors = new List<IdentityError>();

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                IdentityResult? result = await _userManager.CreateAsync(user, request.Password);
                await _context.SaveChangesAsync();
                RegistrationResponse? response;
                if (result.Succeeded)
                {
                    var applicationUser = new AppUser
                    {
                        UserId = new Guid(user.Id).ToString(),
                        Codigo = request.Codigo ?? "00000000",
                        FullName = request.FullName!,
                        Email = request.Email!,
                        Company = request.Company ?? "Maritima Dominicana S.A.S.",
                        Department = request.Department!,
                        Position = request.Position!,
                        Office = request.Office ?? "-",
                    };
                    _context.AppUsers!.Add(applicationUser);
                    await _context.SaveChangesAsync();

                    var token = await GenerateToken(user);

                    transaction.Commit();

                    response = new RegistrationResponse
                    {
                        Email = user.Email!,
                        Token = token.Item1,
                        UserId = user.Id.StringToGuid(),
                        Username = user.UserName!,
                        RefreshToken = token.Item2,
                    };
                }
                else
                {
                    errors = (List<IdentityError>)result.Errors;

                    throw new Exception();
                }

                return response;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                var message = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                if (errors != null)
                {
                    foreach (var item in errors)
                    {
                        message = $"{item.Code}: {item.Description}";
                    }
                }
                throw new SecurityCustomException($"{message}");
            }
        }

        public async Task<AuthResponse> RefreshToken(TokenRequest request)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParamsClone = _tokenValidationParameters.Clone();
            tokenValidationParamsClone.ValidateLifetime = false;

            try
            {
                // validation: El formato del Token es correcto
                var tokenVerification = jwtTokenHandler.ValidateToken(
                    request.Token,
                    tokenValidationParamsClone,
                    out var validatedToken);

                // validation: Verifica encriptacion
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return new AuthResponse
                        {
                            Success = false,
                            Errors = new List<string>
                            {
                                "The Token has encryption errors"
                            }
                        };
                    }
                }

                // validation: Verificar fecha de expiracion
                var utcExpiryDate = long.Parse(
                    tokenVerification!.Claims!.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)!.Value
                    );

                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (_localTime > expiryDate)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "El Token ha expirado"
                        }
                    };
                }

                //validation: El refresh token exista en la base de datos
                var storedToken = await _context.RefreshTokens!.FirstOrDefaultAsync(x => x.Token == request.RefreshToken);
                if (storedToken is null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "El Token no existe"
                        }
                    };
                }

                // validation para verificar si el token ya fue usado

                if (storedToken.IsUsed)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "The token has already been used"
                        }
                    };
                }

                //validation el token fue revocado??
                if (storedToken.IsRevoked)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "The token has been revoked"
                        }
                    };
                }

                //validar el id del token
                var jti = tokenVerification!.Claims!.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)!.Value;

                if (storedToken.JwtId != jti)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "Token does not match initial value!"
                        }
                    };
                }

                // segunda validacion para fecha de expiracion
                if (_localTime > storedToken.ExpireDate)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "El refresh token has expirado!"
                        }
                    };
                }

                storedToken.IsUsed = true;
                _context.RefreshTokens!.Update(storedToken);
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(storedToken.UserId!.ToString());
                var token = await GenerateToken(user!);

                return new AuthResponse
                {
                    Id = user!.Id,
                    Token = token.Item1,
                    Email = user.Email!,
                    Username = user.UserName!,
                    RefreshToken = token.Item2,
                    Success = true,
                };

            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Lifetime validation failed. The token is expired"))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "The Token has expired please you have to log in again"
                        }
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Errors = new List<string>
                            {
                                "The Token has errors, you have to re-login"
                            }
                    };
                }
            }
        }

        private async Task<Tuple<string, string>> GenerateToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_jwtSettings.Key));

            var userClaims = await _userManager.GetClaimsAsync(user);

            var roles = await _userManager.GetRolesAsync(user);


            var roleClaims = new List<Claim>();

            foreach (var role in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(CustomClaimTypes.Uid, user.Id),
                    new Claim(CustomClaimTypes.Sub, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }
                .Union(userClaims)
                .Union(roleClaims)),
                Expires = DateTime.UtcNow.Add(_jwtSettings.ExpireTime),
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                CreatedDate = DateTime.UtcNow,
                ExpireDate = DateTime.UtcNow.AddMonths(6),
                Token = $"{GenerateRandomTokenCharacters(35)}{Guid.NewGuid()}"
            };

            await _context.RefreshTokens!.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new Tuple<string, string>(jwtToken, refreshToken.Token);
        }

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeval = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dateTimeval = dateTimeval.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTimeval;
        }

        private static string GenerateRandomTokenCharacters(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(x => x[random.Next(x.Length)]).ToArray());
        }
    }
}
