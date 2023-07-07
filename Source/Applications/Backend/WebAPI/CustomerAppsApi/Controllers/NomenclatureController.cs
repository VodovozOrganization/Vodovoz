using System;
using System.Collections.Concurrent;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
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
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку цен и остатков от источника {Source}", sourceName);
				var now = DateTime.Now;
				var lastRequestTime = _requestTimes.GetOrAdd(source, now);
				var passedTime = lastRequestTime == now ? 0d : (now - lastRequestTime).TotalMinutes;

				if(passedTime > 0 && passedTime < 1)
				{
					_logger.LogInformation("Превышен интервал обращений для источника {Source}", sourceName);
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
				_logger.LogError(e, "Ошибка при получении цен и остатков для источника {Source}", sourceName);
				return new NomenclaturesPricesAndStockDto
				{
					ErrorMessage = e.Message
				};
			}
		}
	}
}
