using CustomerAppsApi.Library.Dto.Goods;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class NomenclatureController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly INomenclatureModel _nomenclatureModel;
		private readonly PricesFrequencyRequestsHandler _pricesFrequencyRequestsHandler;
		private readonly NomenclaturesFrequencyRequestsHandler _nomenclaturesFrequencyRequestsHandler;

		public NomenclatureController(
			ILogger<CounterpartyController> logger,
			INomenclatureModel nomenclatureModel,
			PricesFrequencyRequestsHandler pricesFrequencyRequestsHandler,
			NomenclaturesFrequencyRequestsHandler nomenclaturesFrequencyRequestsHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_nomenclatureModel = nomenclatureModel ?? throw new ArgumentNullException(nameof(nomenclatureModel));
			_pricesFrequencyRequestsHandler =
				pricesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(pricesFrequencyRequestsHandler));
			_nomenclaturesFrequencyRequestsHandler =
				nomenclaturesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(nomenclaturesFrequencyRequestsHandler));
		}

		[HttpGet("GetNomenclaturesPricesAndStocks")]
		public NomenclaturesPricesAndStockDto GetNomenclaturesPricesAndStocks([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку цен и остатков от источника {Source}", sourceName);

				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				var canRequest = isDryRun || _pricesFrequencyRequestsHandler.CanRequest(source, sourceName);

				if(!canRequest)
				{
					return new NomenclaturesPricesAndStockDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var pricesAndStocks = _nomenclatureModel.GetNomenclaturesPricesAndStocks(source);

				if(!isDryRun)
				{
					_pricesFrequencyRequestsHandler.TryUpdate(source);
				}

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

				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				var canRequest = isDryRun || _nomenclaturesFrequencyRequestsHandler.CanRequest(source, sourceName);

				if(!canRequest)
				{
					return new NomenclaturesDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var nomenclatures = _nomenclatureModel.GetNomenclatures(source);

				if(!isDryRun)
				{
					_nomenclaturesFrequencyRequestsHandler.TryUpdate(source);
				}

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
