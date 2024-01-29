using System;
using System.Collections.Concurrent;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class NomenclatureController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly INomenclatureModel _nomenclatureModel;
		private readonly IConfigurationSection _frequencyMinutesLimitSection;
		private static readonly ConcurrentDictionary<Source, DateTime> _requestPricesTimes = new ConcurrentDictionary<Source, DateTime>();
		private static readonly ConcurrentDictionary<Source, DateTime> _requestNomenclaturesTimes =
			new ConcurrentDictionary<Source, DateTime>();

		public NomenclatureController(
			ILogger<CounterpartyController> logger,
			INomenclatureModel nomenclatureModel,
			IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_nomenclatureModel = nomenclatureModel ?? throw new ArgumentNullException(nameof(nomenclatureModel));
			_frequencyMinutesLimitSection = 
				(configuration ?? throw new ArgumentNullException(nameof(configuration)))
				.GetSection("RequestsMinutesLimits");
		}

		[HttpGet("GetNomenclaturesPricesAndStocks")]
		public NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку цен и остатков от источника {Source}", sourceName);
				var now = DateTime.Now;
				var lastRequestTime = _requestPricesTimes.GetOrAdd(source, now);
				var passedMinutes = lastRequestTime == now ? 0d : (now - lastRequestTime).TotalMinutes;
				var requestFrequencyMinutesLimit = _frequencyMinutesLimitSection.GetValue<int>("PricesAndStocksRequestFrequencyLimit");

				if(passedMinutes > 0 && passedMinutes < requestFrequencyMinutesLimit)
				{
					_logger.LogInformation("Превышен интервал обращений для источника {Source}", sourceName);
					return new NomenclaturesPricesAndStockDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var pricesAndStocks = _nomenclatureModel.GetNomenclaturesPricesAndStocks(source);
				_requestPricesTimes.TryUpdate(source, DateTime.Now, lastRequestTime);
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
		
		[HttpGet("GetNomenclatures")]
		public NomenclaturesDto GetNomenclatures([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку номенклатур от источника {Source}", sourceName);
				var now = DateTime.Now;
				var lastRequestTime = _requestNomenclaturesTimes.GetOrAdd(source, now);
				var passedMinutes = lastRequestTime == now ? 0d : (now - lastRequestTime).TotalMinutes;
				var requestFrequencyMinutesLimit = _frequencyMinutesLimitSection.GetValue<int>("NomenclaturesRequestFrequencyLimit");

				if(passedMinutes > 0 && passedMinutes < requestFrequencyMinutesLimit)
				{
					_logger.LogInformation("Превышен интервал обращений для источника {Source}", sourceName);
					return new NomenclaturesDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var nomenclatures = _nomenclatureModel.GetNomenclatures(source);
				_requestNomenclaturesTimes.TryUpdate(source, DateTime.Now, lastRequestTime);
				return nomenclatures;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении номенклатур для источника {Source}", sourceName);
				return new NomenclaturesDto
				{
					ErrorMessage = e.Message
				};
			}
		}
	}
}
