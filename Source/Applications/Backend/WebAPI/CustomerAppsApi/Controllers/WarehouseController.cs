using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class WarehouseController : ControllerBase
	{
		private readonly ILogger<WarehouseController> _logger;
		private readonly IWarehouseModel _warehouseModel;
		private readonly SelfDeliveriesAddressesFrequencyRequestsHandler _selfDeliveriesAddressesFrequencyRequestsHandler;

		public WarehouseController(
			ILogger<WarehouseController> logger,
			IWarehouseModel counterpartyModel,
			SelfDeliveriesAddressesFrequencyRequestsHandler selfDeliveriesAddressesFrequencyRequestsHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_warehouseModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
			_selfDeliveriesAddressesFrequencyRequestsHandler =
				selfDeliveriesAddressesFrequencyRequestsHandler
				?? throw new ArgumentNullException(nameof(selfDeliveriesAddressesFrequencyRequestsHandler));
		}

		[HttpGet("GetSelfDeliveriesAddresses")]
		public IActionResult GetSelfDeliveriesAddresses([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			
			_logger.LogInformation("Поступил запрос на выборку адресов самовывоза от источника {Source}", sourceName);
			try
			{
				if(!_selfDeliveriesAddressesFrequencyRequestsHandler.CanRequest(source, sourceName))
				{
					return BadRequest("Превышен интервал обращений");
				}

				_selfDeliveriesAddressesFrequencyRequestsHandler.TryUpdate(source);
				return Ok(_warehouseModel.GetSelfDeliveriesAddresses());
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении адресов самовывоза для {Source}",
					sourceName);

				return Problem("Внутренняя ошибка сервера");
			}
		}
	}
}
