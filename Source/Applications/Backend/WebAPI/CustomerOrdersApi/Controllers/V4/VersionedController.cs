using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;

namespace CustomerOrdersApi.Controllers.V4
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion("4.0")]
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
