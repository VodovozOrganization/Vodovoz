using System;
using CustomerAppsApi.Library.V2.Dto.Goods;
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
	public class NomenclatureController : VersionedController
	{
		private readonly INomenclatureModel _nomenclatureModel;
		private readonly PricesFrequencyRequestsHandler _pricesFrequencyRequestsHandler;
		private readonly NomenclaturesFrequencyRequestsHandler _nomenclaturesFrequencyRequestsHandler;

		public NomenclatureController(
			ILogger<CounterpartyController> logger,
			INomenclatureModel nomenclatureModel,
			PricesFrequencyRequestsHandler pricesFrequencyRequestsHandler,
			NomenclaturesFrequencyRequestsHandler nomenclaturesFrequencyRequestsHandler)
			: base(logger) 
		{
			_nomenclatureModel = nomenclatureModel ?? throw new ArgumentNullException(nameof(nomenclatureModel));
			_pricesFrequencyRequestsHandler =
				pricesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(pricesFrequencyRequestsHandler));
			_nomenclaturesFrequencyRequestsHandler =
				nomenclaturesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(nomenclaturesFrequencyRequestsHandler));
		}

		[HttpGet]
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
		
		[HttpGet]
		public SaleItemsDto GetNomenclatures([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку номенклатур от источника {Source}", sourceName);

				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				var canRequest = isDryRun || _nomenclaturesFrequencyRequestsHandler.CanRequest(source, sourceName);

				if(!canRequest)
				{
					return new SaleItemsDto
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
				return new SaleItemsDto
				{
					ErrorMessage = e.Message
				};
			}
		}
	}
}
