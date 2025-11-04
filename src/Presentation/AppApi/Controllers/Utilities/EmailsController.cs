using System.Net;
using Application.DTOs;
using Application.Features.Utilities.SendMails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AppApi.Controllers.Utilities
{
    [SwaggerTag]
    public class EmailsController : ApiBaseController<EmailsController>
    {
        public EmailsController(IMediator mediator, ILogger<EmailsController> logger) : base(mediator, logger) { }

        [HttpPost("sendMail")]
        [SwaggerOperation(Summary = "Envia un Email.",
            Description = "")]
        [ProducesResponseType(typeof(Result<bool>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> SendMail(SendMailCommand mail)
        {
            return Ok(await _mediator.Send(mail));
        }

        [HttpPost("sendMailNotification")]
        [SwaggerOperation(Summary = "Envia un Email de Tipo Notificacion.",
            Description = "")]
        [ProducesResponseType(typeof(Result<bool>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> SendMailNotification(SendMailNotificationCommand mail)
        {
            return Ok(await _mediator.Send(mail));
        }
    }
}
