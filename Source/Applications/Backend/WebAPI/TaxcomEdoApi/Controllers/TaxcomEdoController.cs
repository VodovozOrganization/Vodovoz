using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TaxcomEdoController : ControllerBase
	{
		private readonly ILogger<TaxcomEdoController> _logger;

		public TaxcomEdoController(ILogger<TaxcomEdoController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		[Route("/Login")]
		public void Login()
		{
			
		}
	}
}
