using System.Net;
using AppApi.Authorization;
using Application.DTOs;
using Application.Features.Examples.Products.Commands;
using Application.Features.Examples.Products.Queries;
using Application.Features.Examples.Products.VMs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Persistence.Pagination;
using Swashbuckle.AspNetCore.Annotations;

namespace AppApi.Controllers.Examples
{
    /// <summary>
    /// Controller for managing product operations.
    /// Handles CRUD operations for products using CQRS pattern via MediatR.
    /// </summary>
    [CustomAuthorize]
    [SwaggerTag]
    public class ProductsController : ApiBaseController<ProductsController>
    {
        /// <summary>
        /// Initializes a new instance of the ProductsController
        /// </summary>
        /// <param name="mediator">MediatR instance for sending commands and queries</param>
        /// <param name="logger">Logger instance for ProductsController</param>
        public ProductsController(IMediator mediator, ILogger<ProductsController> logger) : base(mediator, logger) { }

        [HttpGet("getAll")]
        [SwaggerOperation(Summary = "Retornar Todos los productos.",
          Description = "Enpoint que retorna todas los Productos creados en la base de datos.")]
        [ProducesResponseType(typeof(Result<List<ProductVm>>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<Result<List<ProductVm>>>> GetAll([FromQuery] bool? withCache = false)
        {
            var Products = await _mediator.Send(new GetAllProductsQuery() { EnableCache = withCache });

            return Ok(Products);
        }

        [HttpGet("getById")]
        [SwaggerOperation(Summary = "Retorna una producto en base a su Id.",
            Description = "Busca en la base de datos el producto por el Id enviado en el request.")]
        [ProducesResponseType(typeof(Result<ProductVm>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<Result<ProductVm>>> GetById([FromQuery] string? pkId)
        {
            var Products = await _mediator.Send(new GetProductByIdQuery() { PkId = pkId });

            return Ok(Products);
        }

        [HttpGet("GetByIdWithCategory")]
        [SwaggerOperation(Summary = "Retorna una producto junto a su categoria en base a su Id.",
            Description = "Busca en la base de datos el producto por el Id enviado en el request y retorna el mismo con el detalle de su categoria asociada.")]
        [ProducesResponseType(typeof(Result<ProductVm>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<Result<ProductVm>>> GetByIdWithCategory([FromQuery] string? pkId)
        {
            var Products = await _mediator.Send(new GetProductWithCategoryByIdQuery() { Id = pkId });

            return Ok(Products);
        }

        [HttpGet("pagination", Name = "productPaginate")]
        [SwaggerOperation(Summary = "Retorna todas los productos Paginados.",
            Description = "Trae todos los productos paginados, y se puede filtral el Titulo del producto con el parametro [search].")]
        [ProducesResponseType(typeof(PaginationVm<ProductVm>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<ActionResult<PaginationVm<ProductVm>>> GetProductsPaginated([FromQuery] GetPaginatedProductsQuery parameters)
        {
            var dataResut = await _mediator.Send(parameters);
            return Ok(dataResut);
        }

        [HttpPost("create")]
        [SwaggerOperation(Summary = "Crea nueva producto.",
            Description = "Enpoint que Crea una nueva producto en la Base de Datos.")]
        [ProducesResponseType(typeof(Result<string>), (int)HttpStatusCode.Created)]
        [Produces("application/json")]
        public async Task<IActionResult> Create(CreateProductCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpPut("update")]
        [SwaggerOperation(Summary = "Actualizar Datos de una producto.",
            Description = "Enpoint que Actiza los datos de una Producto en base al su ID.")]
        [ProducesResponseType(typeof(Result<string>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Update(UpdateProductCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpDelete("delete")]
        [SwaggerOperation(Summary = "Elimina una producto.",
            Description = "Enpoint que Elimina los datos de una Producto en base al su ID.")]
        [ProducesResponseType(typeof(Result<string>), (int)HttpStatusCode.OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string id)
        {
            return Ok(await _mediator.Send(new DeleteProductCommand { Id = id }));
        }
    }
}
