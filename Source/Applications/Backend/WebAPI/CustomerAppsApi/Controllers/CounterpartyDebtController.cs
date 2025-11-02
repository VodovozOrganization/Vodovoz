using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class CounterpartyDebtController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly ICounterpartyModel _counterpartyModel;
		
		public CounterpartyDebtController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
		}
		
		[HttpGet]
		[Route("GetCounterpartyBottlesDebt")]
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
