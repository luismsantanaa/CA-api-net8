using System.Net;
using AppApi.Authorization;
using Application.DTOs;
using Application.Features.Examples.Categories.Commands;
using Application.Features.Examples.Categories.Queries;
using Application.Features.Examples.Categories.VMs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence.Pagination;
using Swashbuckle.AspNetCore.Annotations;

namespace AppApi.Controllers.Examples
{
    [CustomAuthorize]
    [SwaggerTag]
    public class CategoriesController : ApiBaseController<CategoriesController>
    {
        public CategoriesController(IMediator mediator, ILogger<CategoriesController> logger) : base(mediator, logger) { }

        [HttpGet("getAll")]
        [SwaggerOperation(Summary = "Retornar Todas la Categoria.",
            Description = "Enpoint que retorna todas las Categorias creadas en la base de datos.")]
        [ProducesResponseType(typeof(Result<List<CategoryVm>>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<Result<List<CategoryVm>>>> GetAll([FromQuery] bool? withCache = false)
        {
            var Categories = await _mediator.Send(new GetAllCategoriesQuery() { EnableCache = withCache });

            return Ok(Categories);
        }

        [HttpGet("getById")]
        [SwaggerOperation(Summary = "Retorna una Categoria en base a su Id.",
            Description = "Busca en la base de datos la categoria por el Id enviado en el request.")]
        [ProducesResponseType(typeof(Result<CategoryVm>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<Result<CategoryVm>>> GetById([FromQuery] string? pkId)
        {
            var Categories = await _mediator.Send(new GetCategoryByIdQuery() { PkId = pkId });

            return Ok(Categories);
        }

        [HttpGet("pagination", Name = "categoryPaginate")]
        [SwaggerOperation(Summary = "Retorna todas las Categorias Paginadas.",
            Description = "Trae todas las categorias paginadas, y se puede filtral el nombre con el parametro [search].")]
        [ProducesResponseType(typeof(PaginationVm<CategoryVm>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<PaginationVm<CategoryVm>>> GetCatetegoriesPaginated([FromQuery] GetPaginatedCategoriesQuery parameters)
        {
            var dataResut = await _mediator.Send(parameters);
            return Ok(dataResut);
        }

        [HttpPost("create")]
        [SwaggerOperation(Summary = "Crea nueva categoria.",
            Description = "Enpoint que Crea una nueva categoria en la Base de Datos.")]
        [ProducesResponseType(typeof(Result<string>), (int)HttpStatusCode.Created)]
        [Produces("application/json")]
        public async Task<IActionResult> Create(CreateCategoryCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("update")]
        [SwaggerOperation(Summary = "Actualizar Datos de una categoria.",
            Description = "Enpoint que Actiza los datos de una catageria en base al su ID.")]
        [ProducesResponseType(typeof(Result<string>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Update(UpdateCategoryCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("delete")]
        [SwaggerOperation(Summary = "Elimina una categoria.",
            Description = "Enpoint que Elimina los datos de una catageria en base al su ID.")]
        [ProducesResponseType(typeof(Result<string>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string pkId)
        {

            return Ok(await _mediator.Send(new DeleteCategoryCommand { Id = pkId }));
        }
    }
}
