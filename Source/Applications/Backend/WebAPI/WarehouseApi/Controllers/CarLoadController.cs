using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WarehouseApi.Contracts.Responses;

namespace WarehouseApi.Controllers
{
	//[Authorize(Roles = _rolesToAccess)]
	[ApiController]
	[Route("/api/")]
	public class CarLoadController : ControllerBase
	{
		private readonly ILogger<CarLoadController> _logger;

		public CarLoadController(
			ILogger<CarLoadController> logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		[HttpPost("StartLoad")]
		public async Task<IActionResult> StartLoad([FromQuery] int documentId)
		{
			var response = new StartLoadResponse();
			response.Result = Contracts.OperationResult.Success;
			response.CarLoadDocument = new Contracts.Dto.CarLoadDocumentDto
			{
				Id = documentId,
				Driver = "Super Driver",
				Car = "Super Car",
				LoadPriority = 12,
				State = Contracts.CarLoadDocumentState.InProgress
			};

			return Ok(response);
		}

		//[HttpGet("GetOrder")]
		//public async Task<IActionResult> GetOrder([FromQuery] int orderId)
		//{
		//}

		//[HttpPost("AddOrderCode")]
		//public async Task<IActionResult> AddOrderCode()
		//{
		//}

		//[HttpPost("ChangeOrderCode")]
		//public async Task<IActionResult> ChangeOrderCode()
		//{
		//}

		//[HttpPost("EndLoad")]
		//public async Task<IActionResult> EndLoad([FromQuery] int documentId)
		//{
		//}
	}
}
