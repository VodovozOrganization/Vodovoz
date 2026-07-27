using System;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using CustomerAppsApi.Library.V2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	public class CounterpartyController : VersionedController
	{
		private readonly ICounterpartyModel _counterpartyModel;

		public CounterpartyController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel) : base(logger)
		{
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
		}

		[HttpPost]
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
