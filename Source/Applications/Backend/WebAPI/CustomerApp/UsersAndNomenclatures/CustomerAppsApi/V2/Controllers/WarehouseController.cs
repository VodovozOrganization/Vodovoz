using System;
using CustomerAppsApi.Library.V2.Models;
using CustomerAppsApi.Library.V2.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	public class WarehouseController : VersionedController
	{
		private readonly IWarehouseModel _warehouseModel;
		private readonly SelfDeliveriesAddressesFrequencyRequestsHandler _selfDeliveriesAddressesFrequencyRequestsHandler;

		public WarehouseController(
			ILogger<WarehouseController> logger,
			IWarehouseModel counterpartyModel,
			SelfDeliveriesAddressesFrequencyRequestsHandler selfDeliveriesAddressesFrequencyRequestsHandler)
			: base(logger)
		{
			_warehouseModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
			_selfDeliveriesAddressesFrequencyRequestsHandler =
				selfDeliveriesAddressesFrequencyRequestsHandler
				?? throw new ArgumentNullException(nameof(selfDeliveriesAddressesFrequencyRequestsHandler));
		}

		[HttpGet]
		public IActionResult GetSelfDeliveriesAddresses([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			
			_logger.LogInformation("Поступил запрос на выборку адресов самовывоза от источника {Source}", sourceName);
			try
			{
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				var canRequest = isDryRun || _selfDeliveriesAddressesFrequencyRequestsHandler.CanRequest(source, sourceName);

				if(!canRequest)
				{
					return BadRequest("Превышен интервал обращений");
				}

				if(!isDryRun)
				{
					_selfDeliveriesAddressesFrequencyRequestsHandler.TryUpdate(source);
				}

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
