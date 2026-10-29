using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Goods;
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
	public class SaleItemController : VersionedController
	{
		private readonly ISaleItemService _saleItemService;
		private readonly PricesFrequencyRequestsHandler _pricesFrequencyRequestsHandler;
		private readonly NomenclaturesFrequencyRequestsHandler _nomenclaturesFrequencyRequestsHandler;

		public SaleItemController(
			ILogger<SaleItemController> logger,
			ISaleItemService saleItemService,
			PricesFrequencyRequestsHandler pricesFrequencyRequestsHandler,
			NomenclaturesFrequencyRequestsHandler nomenclaturesFrequencyRequestsHandler)
			: base(logger) 
		{
			_saleItemService = saleItemService ?? throw new ArgumentNullException(nameof(saleItemService));
			_pricesFrequencyRequestsHandler =
				pricesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(pricesFrequencyRequestsHandler));
			_nomenclaturesFrequencyRequestsHandler =
				nomenclaturesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(nomenclaturesFrequencyRequestsHandler));
		}

		[HttpGet]
		public async Task<SaleItemsPricesAndStockDto> GetSaleItemsPricesAndStocks(
			[FromQuery] Source source,
			CancellationToken cancellationToken
			)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку цен и остатков от источника {Source}", sourceName);

				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
				var canRequest = isDryRun || _pricesFrequencyRequestsHandler.CanRequest(source, sourceName);

				if(!canRequest)
				{
					return new SaleItemsPricesAndStockDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var pricesAndStocks = await _saleItemService.GetSaleItemsPricesAndStocksAsync(source, cancellationToken);

				if(!isDryRun)
				{
					_pricesFrequencyRequestsHandler.TryUpdate(source);
				}

				return pricesAndStocks;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении цен и остатков для источника {Source}", sourceName);
				return new SaleItemsPricesAndStockDto
				{
					ErrorMessage = e.Message
				};
			}
		}
		
		[HttpGet]
		public async Task<SaleItemsDto> GetSaleItems([FromQuery] Source source, CancellationToken cancellationToken)
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

				var saleItems = await _saleItemService.GetSaleItemsAsync(source, cancellationToken);

				if(!isDryRun)
				{
					_nomenclaturesFrequencyRequestsHandler.TryUpdate(source);
				}

				return saleItems;
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
