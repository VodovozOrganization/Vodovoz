using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;

namespace BitrixApi.Controllers.V1
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	[Authorize]
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
