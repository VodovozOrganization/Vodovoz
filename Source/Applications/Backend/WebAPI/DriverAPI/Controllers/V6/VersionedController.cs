using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion("6.0")]
	[Route("api/v{version:apiVersion}/[action]")]
	[ApiController]
	public class VersionedController : ApiControllerBase
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		public VersionedController(ILogger<ApiControllerBase> logger) : base(logger)
		{
		}
	}
}
