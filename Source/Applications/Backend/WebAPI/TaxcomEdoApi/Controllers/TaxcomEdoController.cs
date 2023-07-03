using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TaxcomEdoApi.Services;

namespace TaxcomEdoApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TaxcomEdoController : ControllerBase
	{
		private readonly ILogger<TaxcomEdoController> _logger;
		private readonly DocumentFlowService _documentFlowService;

		public TaxcomEdoController(ILogger<TaxcomEdoController> logger, DocumentFlowService documentFlowService)
		{
			_logger = logger;
			_documentFlowService = documentFlowService;
		}

		[HttpGet]
		[Route("/Login")]
		public void Login()
		{

		}

		[HttpGet]
		[Route("/SendUpdByOrder")]
		public async Task SendUpdByOrder(int orderId)
		{
			_logger.LogInformation("Запрос на формирование и отправку УПД для заказа №{OrderId}", orderId);
			await _documentFlowService.CreateAndSendUpdByOrderAsync(orderId);
		}
	}
}
