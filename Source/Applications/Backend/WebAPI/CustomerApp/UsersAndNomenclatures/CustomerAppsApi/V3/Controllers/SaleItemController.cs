using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V3.Dto.Goods;
using CustomerAppsApi.Library.V3.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V3.Controllers
{
	/// <summary>
	/// Контроллер цен и остатков третьей версии API
	/// </summary>
	[Authorize]
	[ApiVersion("3.0")]
	public class SaleItemController : VersionedController
	{
		private readonly ISaleItemService _saleItemService;
		private readonly PricesFrequencyRequestsHandler _pricesFrequencyRequestsHandler;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="saleItemService">Сервис цен, остатков и бонусов</param>
		/// <param name="pricesFrequencyRequestsHandler">Ограничитель частоты запросов цен</param>
		/// <exception cref="ArgumentNullException">Одна из зависимостей равна <c>null</c></exception>
		public SaleItemController(
			ILogger<SaleItemController> logger,
			ISaleItemService saleItemService,
			PricesFrequencyRequestsHandler pricesFrequencyRequestsHandler)
			: base(logger)
		{
			_saleItemService = saleItemService ?? throw new ArgumentNullException(nameof(saleItemService));
			_pricesFrequencyRequestsHandler =
				pricesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(pricesFrequencyRequestsHandler));
		}

		/// <summary>
		/// Возвращает цены и остатки продаваемых товаров/услуг.
		/// Для мобильного приложения каждая позиция дополнительно содержит
		/// количество бонусов, начисляемых за 1 шт. по базовой цене.
		/// </summary>
		/// <param name="source">Источник запроса (ИПЗ)</param>
		/// <param name="cancellationToken">Токен отмены запроса</param>
		/// <returns>Список цен и остатков</returns>
		[HttpGet]
		public async Task<SaleItemsPricesAndStockDto> GetSaleItemsPricesAndStocks(
			[FromQuery] Source source,
			CancellationToken cancellationToken)
		{
			var sourceName = source.GetEnumTitle();

			try
			{
				_logger.LogInformation(
					"Поступил запрос на выборку цен и остатков от источника {Source}",
					sourceName);

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
				_logger.LogError(
					e,
					"Ошибка при получении цен и остатков для источника {Source}",
					sourceName);

				return new SaleItemsPricesAndStockDto
				{
					ErrorMessage = e.Message
				};
			}
		}
	}
}
