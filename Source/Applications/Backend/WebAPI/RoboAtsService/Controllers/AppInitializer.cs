using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RoboAtsService.Controllers
{
	[ApiController]
	[Route("api")]
	public class AppInitializer
	{
		private readonly ILogger<AppInitializer> _logger;

		public AppInitializer(ILogger<AppInitializer> logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		[HttpGet]
		[Route("init")]
		[AllowAnonymous]
		public string AppInit()
		{
			_logger.LogInformation("Application initialized");
			return "true";
		}
	}
}
