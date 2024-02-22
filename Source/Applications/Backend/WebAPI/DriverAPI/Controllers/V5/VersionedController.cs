using DriverApi.Contracts.V5;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion("5.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	public class VersionedController : ControllerBase
	{
		/// <summary>
		/// Метод возврата ошибки
		/// </summary>
		/// <param name="message"></param>
		/// <param name="statusCode"></param>
		/// <returns></returns>
		protected IActionResult Error(string message, int statusCode = StatusCodes.Status400BadRequest)
		{
			HttpContext.Response.StatusCode = statusCode;
			return BadRequest(new ErrorResponseDto(message));
		}
	}
}
