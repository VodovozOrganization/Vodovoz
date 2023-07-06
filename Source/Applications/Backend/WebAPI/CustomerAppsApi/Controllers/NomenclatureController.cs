using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Controllers
{
	public class NomenclatureController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly INomenclatureModel _nomenclatureModel;
		private static readonly ConcurrentDictionary<Source, DateTime> _requestTimes = new ConcurrentDictionary<Source, DateTime>();

		public NomenclatureController(
			ILogger<CounterpartyController> logger,
			INomenclatureModel nomenclatureModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_nomenclatureModel = nomenclatureModel ?? throw new ArgumentNullException(nameof(nomenclatureModel));
		}

		[HttpGet]
		[Route("GetNomenclaturesPricesAndStocks")]
		public NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks([FromQuery] Source source)
		{
			try
			{
				var lastRequestTime = _requestTimes.GetOrAdd(source, DateTime.Now);
				var passedTime = (lastRequestTime - DateTime.Now).TotalMinutes;

				if(passedTime < 1)
				{
					return new NomenclaturesPricesAndStockDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var pricesAndStocks = _nomenclatureModel.GetNomenclaturesPricesAndStocks(source);
				_requestTimes.TryUpdate(source, DateTime.Now, lastRequestTime);
				return pricesAndStocks;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении цен и остатков для {Source}", source);
				return new NomenclaturesPricesAndStockDto
				{
					ErrorMessage = e.Message
				};
			}
		}
	}
}
