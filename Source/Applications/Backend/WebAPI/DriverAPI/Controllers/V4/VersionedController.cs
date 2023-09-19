using Microsoft.AspNetCore.Mvc;

namespace DriverAPI.Controllers.V4
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion("4.0")]
	[Route("api/v{version:apiVersion}")]
	[ApiController]
	public class VersionedController : ControllerBase
	{
	}
}
