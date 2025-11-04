using System.Net;
using Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Security.Entities.DTOs;
using Security.Services.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace AppApi.Controllers.Auth
{
    [SwaggerTag("User Management.")]
    public class UsersController : ApiBaseController<UsersController>
    {
        private readonly IMardomAuthService _userService;

        public UsersController(IMediator mediator, ILogger<UsersController> logger, IMardomAuthService userService) : base(mediator, logger)
        {
            _userService = userService;
        }

        [HttpPost]
        [Route("ChangePassword")]
        [SwaggerOperation(Summary = "Realizar el cambio de Password.",
            Description = "Endpoint que solicita el cambio de Password en el API de Seguridad.")]
        [ProducesResponseType(typeof(Result<bool>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<Result<bool>>> ChangePassword([FromBody] ChangePassword input)
        {
            var userChangePassword = await _userService.ChangePassword(input);

            return Ok(userChangePassword);
        }

        [HttpGet]
        [Route("ResetPassword/{email}")]
        [SwaggerOperation(Summary = "Realizar el Reset de Password.",
            Description = "Endpoint que realiza el proceso para cambio de password cuando se ha olvidado este.")]
        [ProducesResponseType(typeof(Result<bool>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<Result<bool>>> ResetPassword(string email)
        {
            var userResetPassword = await _userService.ResetPassword(email);

            return Ok(userResetPassword);
        }
    }
}
