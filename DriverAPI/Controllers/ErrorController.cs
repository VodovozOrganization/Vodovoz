using DriverAPI.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        [Route("/api/error")]
        public ErrorResponseModel Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error ?? new System.Exception("Exception");
            var code = 500;

            Response.StatusCode = code;

            return new ErrorResponseModel(exception.Message);
        }
    }
}
