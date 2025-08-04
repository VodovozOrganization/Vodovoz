using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion("1.0")]
	[Route("api/[action]")]
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
