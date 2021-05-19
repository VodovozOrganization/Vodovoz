using DriverAPI.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger<ErrorController> logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("/api/error")]
        public ErrorResponseModel Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;
            var code = 500;

            if (exception != null)
            {
                logger.LogError(exception, exception.Message);
            }
            else
            {
                exception = new System.Exception("Вызван обработчик ошибок без ошибки");
            }

            Response.StatusCode = code;

            return new ErrorResponseModel(exception.Message);
        }
    }
}
