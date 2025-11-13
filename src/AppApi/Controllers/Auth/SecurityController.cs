using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Entities;
using Security.Entities.DTOs;
using Security.Services.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace AppApi.Controllers.Auth
{
    [SwaggerTag]
    public class SecurityController : ApiBaseController<SecurityController>
    {
        private readonly IAppAuthService _appAuthService;
        private readonly IMardomAuthService _mardomAuthService;

        public SecurityController(IMediator mediator, ILogger<SecurityController> logger, IAppAuthService authService, IMardomAuthService mardomAuthService) : base(mediator, logger)
        {
            _appAuthService = authService;
            _mardomAuthService = mardomAuthService;
        }

        [HttpPost("LoginMardom")]
        [SwaggerOperation(Summary = "Hacer Login y recibir el Token.",
           Description = "Enpoint que realiza el login en el API de seguridad y retorna el Token Generado.")]
        [ProducesResponseType(typeof(AuthResponse), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<AuthResponse>> LoginMardom([FromBody] AuthRequest request)
        {
            return Ok(await _mardomAuthService.UserAuthentication(request));
        }

        [HttpPost("Login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] AuthRequest request)
        {
            return Ok(await _appAuthService.UserAuthentication(request));
        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegistrationResponse>> Register([FromBody] RegistrationRequest request)
        {
            return Ok(await _appAuthService.Register(request));
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] TokenRequest request)
        {
            return Ok(await _appAuthService.RefreshToken(request));
        }

    }
}
