using System;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using CustomerAppsApi.Library.V2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	public class CounterpartyDebtController : VersionedController
	{
		private readonly ICounterpartyModel _counterpartyModel;

		public CounterpartyDebtController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel) : base(logger)
		{
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
		}
		
		[HttpGet]
		public CounterpartyBottlesDebtDto GetCounterpartyBottlesDebt([FromQuery] int erpCounterpartyId)
		{
			try
			{
				return _counterpartyModel.GetCounterpartyBottlesDebt(erpCounterpartyId);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении долга по бутылям контрагента {CounterpartyId}",
					erpCounterpartyId);
				throw;
			}
		}
	}
}
