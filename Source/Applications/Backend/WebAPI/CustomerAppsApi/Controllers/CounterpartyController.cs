using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class CounterpartyController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly ICounterpartyModel _counterpartyModel;

		public CounterpartyController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
		}

		[HttpPost]
		[Route("GetCounterparty")]
		public CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto)
		{
			try
			{
				return _counterpartyModel.GetCounterparty(counterpartyContactInfoDto);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при идентификации контрагента {ExternalCounterpartyId}",
					counterpartyContactInfoDto.ExternalCounterpartyId);
				throw;
			}
		}
		
		[HttpPost]
		[Route("RegisterCounterparty")]
		public CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto)
		{
			try
			{
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
				
				return _counterpartyModel.RegisterCounterparty(counterpartyDto, isDryRun);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при регистрации контрагента {ExternalCounterpartyId}",
					counterpartyDto.ExternalCounterpartyId);
				throw;
			}
		}
		
		[HttpPost]
		[Route("UpdateCounterpartyInfo")]
		public CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			try
			{
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
				
				return _counterpartyModel.UpdateCounterpartyInfo(counterpartyDto, isDryRun);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при обновлении данных контрагента");
				throw;
			}
		}
	}
}
