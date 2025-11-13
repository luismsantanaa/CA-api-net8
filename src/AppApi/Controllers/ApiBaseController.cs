using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AppApi.Controllers
{
    /// <summary>
    /// Base controller for all API controllers in the application.
    /// Provides common functionality including MediatR for CQRS pattern and logging.
    /// </summary>
    /// <typeparam name="T">The controller type for logging purposes</typeparam>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiBaseController<T> : ControllerBase
    {
        /// <summary>
        /// MediatR instance for sending commands and queries
        /// </summary>
        protected readonly IMediator _mediator;
        
        /// <summary>
        /// Logger instance for the specific controller type
        /// </summary>
        protected readonly ILogger<T> _logger;

        /// <summary>
        /// Initializes a new instance of the ApiBaseController class
        /// </summary>
        /// <param name="mediator">MediatR instance for CQRS operations</param>
        /// <param name="logger">Logger instance for the controller</param>
        protected ApiBaseController(IMediator mediator, ILogger<T> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
    }
}
