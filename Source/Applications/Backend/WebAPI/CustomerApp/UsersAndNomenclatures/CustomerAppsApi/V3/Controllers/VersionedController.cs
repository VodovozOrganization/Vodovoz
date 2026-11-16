using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;

namespace CustomerAppsApi.V3.Controllers
{
	/// <summary>
	/// Базовый контроллер третьей версии API
	/// </summary>
	[Route("api/v{version:apiVersion}/[action]")]
	[ApiController]
	public class VersionedController : ApiControllerBase
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		public VersionedController(ILogger<ApiControllerBase> logger) : base(logger)
		{
		}
	}
}
