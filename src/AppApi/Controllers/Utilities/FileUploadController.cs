using System.Net;
using Application.DTOs;
using Application.Features.Utilities.UploadFiles.Comands.Create;
using Application.Features.Utilities.UploadFiles.Comands.VoidFile;
using Application.Features.Utilities.UploadFiles.Queries;
using Application.Features.Utilities.UploadFiles.Queries.Vm;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AppApi.Controllers.Utilities
{
    [SwaggerTag]
    public class FilesUploadController : ApiBaseController<FilesUploadController>
    {
        public FilesUploadController(IMediator mediator, ILogger<FilesUploadController> logger) : base(mediator, logger) { }

        [HttpGet("GetById")]
        [SwaggerOperation(Summary = "Retorna el registro de un Archivo subido al servidor en base a su Id.",
            Description = "")]
        [ProducesResponseType(typeof(Result<UploadedFileVm>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetById(string id)
        {
            var company = await _mediator.Send(new GetUploadedFileByIdQuery() { PkId = id });

            return Ok(company);
        }

        [HttpGet("GetByReference")]
        [SwaggerOperation(Summary = "Retorna el registro de un Archivo subido al servidor en base a su referencia o su nombre.",
            Description = "")]
        [ProducesResponseType(typeof(Result<UploadedFileVm>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> GetByReference(string reference)
        {
            var company = await _mediator.Send(new GetUploadFileByReferenceQuery() { Reference = reference });

            return Ok(company);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Sube un archivo al servidor y crea un registro en la base de datos.",
            Description = "")]
        [ProducesResponseType(typeof(Result<bool>), (int)HttpStatusCode.Created)]
        [Produces("application/json")]
        public async Task<IActionResult> Create(List<IFormFile> files,
            string? type = null, string? reference = null, string? comment = null)
        {
            return Ok(await _mediator.Send(new UploadFileCommand()
            {
                Files = files,
                Type = type,
                Reference = reference,
                Comment = comment
            }));
        }

        [HttpDelete]
        [SwaggerOperation(Summary = "Elimina/Inactiva un registro de un archivo subido al servidor.",
            Description = "")]
        [ProducesResponseType(typeof(Result<string>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string id)
        {
            return Ok(await _mediator.Send(new VoidUploadedFileCommand { Id = id }));
        }
    }
}
