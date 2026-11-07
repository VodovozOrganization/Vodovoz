using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V3.Converters;
using CustomerAppsApi.Library.V3.Dto.Goods;
using CustomerAppsApi.Library.V3.Factories;
using CustomerAppsApi.Library.V3.Repositories;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Mapping;
using Mindbox.Library.Services;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Settings.Common;

namespace CustomerAppsApi.Library.V3.Services
{
	/// <inheritdoc />
	public class SaleItemService : ISaleItemService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGeneralSettings _generalSettings;
		private readonly ISaleItemRepository _saleItemRepository;
		private readonly ISourceConverter _sourceConverter;
		private readonly ISaleItemFactory _saleItemFactory;
		private readonly IMindboxBonusService _mindboxBonusService;
		private readonly ILogger<SaleItemService> _logger;

		/// <summary>
		/// Конструктор сервиса цен, остатков и бонусов
		/// </summary>
		/// <param name="uowFactory">Фабрика unit of work</param>
		/// <param name="generalSettings">Общие настройки</param>
		/// <param name="saleItemRepository">Репозиторий продаваемых позиций</param>
		/// <param name="sourceConverter">Конвертер источника запроса</param>
		/// <param name="saleItemFactory">Фабрика DTO продаваемых позиций</param>
		/// <param name="mindboxBonusService">Сервис расчёта бонусов в Mindbox</param>
		/// <param name="logger">Логгер</param>
		/// <exception cref="ArgumentNullException">Одна из зависимостей равна <c>null</c></exception>
		public SaleItemService(
			IUnitOfWorkFactory uowFactory,
			IGeneralSettings generalSettings,
			ISaleItemRepository saleItemRepository,
			ISourceConverter sourceConverter,
			ISaleItemFactory saleItemFactory,
			IMindboxBonusService mindboxBonusService,
			ILogger<SaleItemService> logger)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_saleItemRepository = saleItemRepository ?? throw new ArgumentNullException(nameof(saleItemRepository));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_saleItemFactory = saleItemFactory ?? throw new ArgumentNullException(nameof(saleItemFactory));
			_mindboxBonusService = mindboxBonusService ?? throw new ArgumentNullException(nameof(mindboxBonusService));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <inheritdoc/>
		public async Task<SaleItemsPricesAndStockDto> GetSaleItemsPricesAndStocksAsync(
			Source source,
			CancellationToken cancellationToken)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;

			using var batchUow = _uowFactory.CreateWithoutRoot();
			using var stocksUow = _uowFactory.CreateWithoutRoot();
			using var promoSetItemsUow = _uowFactory.CreateWithoutRoot();
			var aggregatedDataTask = _saleItemRepository.GetAggregatedSaleItemPricesAsync(
				batchUow, parameterType, cancellationToken);
			//TODO доработать выборку остатков и данных по товарам промонаборов. Они самые тяжелые выполняются от 6 до 10сек
			var stocksTask = _saleItemRepository.GetNomenclaturesForSendInStockAsync(
				stocksUow, parameterType, warehouses, cancellationToken);
			var promoSetItemsPricesTask = _saleItemRepository.GetPromotionalSetsItemsWithBalanceForSendAsync(
				promoSetItemsUow, parameterType, warehouses, cancellationToken);

			await Task.WhenAll(
				aggregatedDataTask,
				stocksTask,
				promoSetItemsPricesTask
				);

			var aggregatedData = aggregatedDataTask
				.Result
				.AddNomenclatureStocks(stocksTask.Result)
				.AddPromoSetItemPrices(promoSetItemsPricesTask.Result);

			var allSaleItemPrices = new List<SaleItemPricesDto>();

			allSaleItemPrices.AddRange(ProcessNomenclatures(aggregatedData));
			allSaleItemPrices.AddRange(ProcessPromoSets(aggregatedData));
			allSaleItemPrices.AddRange(aggregatedData.RentPackagePrices);

			var result = _saleItemFactory.CreateSaleItemsPricesAndStockDto(allSaleItemPrices);

			if(source == Source.MobileApp)
			{
				await FillBonusAmountsAsync(result.PricesAndStocks, cancellationToken);
			}

			return result;
		}

		/// <inheritdoc/>
		public async Task<SaleItemsDto> GetSaleItemsAsync(Source source, CancellationToken cancellationToken)
		{
			using var uow = _uowFactory.CreateWithoutRoot();
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var data = await _saleItemRepository.GetAggregatedSaleItemsAsync(uow, parameterType, cancellationToken);

			var availableWaterIds = data.Nomenclatures
				.Where(x => x.Category == NomenclatureCategory.water)
				.Where(x => x.TareVolume == TareVolume.Vol19L)
				.Select(x => x.ErpId)
				.ToList();

			return _saleItemFactory.CreateSaleItemsDto(data, availableWaterIds);
		}

		private async Task FillBonusAmountsAsync(
			IEnumerable<SaleItemPricesDto> saleItemPrices,
			CancellationToken cancellationToken)
		{
			var requestItems = new List<MindboxBonusRequestItem>();
			var itemsByKey = new Dictionary<string, SaleItemPricesDto>(StringComparer.Ordinal);

			foreach(var saleItemPrice in saleItemPrices)
			{
				var basePrice = GetBasePrice(saleItemPrice);

				if(!basePrice.HasValue)
				{
					continue;
				}

				string itemKey;

				try
				{
					itemKey = MindboxItemIdBuilder.Build(saleItemPrice.Type, saleItemPrice.ErpId);
				}
				catch(ArgumentOutOfRangeException)
				{
					continue;
				}

				if(itemsByKey.ContainsKey(itemKey))
				{
					continue;
				}

				itemsByKey[itemKey] = saleItemPrice;
				requestItems.Add(new MindboxBonusRequestItem(itemKey, basePrice.Value));
			}

			if(requestItems.Count == 0)
			{
				return;
			}

			try
			{
				var bonuses = await _mindboxBonusService.CalculateBonusesAsync(requestItems, cancellationToken);

				foreach(var bonus in bonuses)
				{
					if(itemsByKey.TryGetValue(bonus.Key, out var saleItemPrice))
					{
						saleItemPrice.BonusAmount = bonus.Value;
					}
				}
			}
			catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
			{
				throw;
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка расчёта бонусов Mindbox. Ответ по ценам и остаткам будет без бонусов");
			}
		}

		private static decimal? GetBasePrice(SaleItemPricesDto saleItemPrice)
		{
			SaleItemPriceDto basePrice = null;

			if(saleItemPrice.Prices is null)
			{
				return null;
			}

			foreach(var price in saleItemPrice.Prices)
			{
				if(price is null)
				{
					continue;
				}

				if(basePrice is null || price.MinCount < basePrice.MinCount)
				{
					basePrice = price;
				}
			}

			return basePrice?.Price;
		}

		private IEnumerable<SaleItemPricesDto> ProcessNomenclatures(AggregatedSaleItemPrices aggregatedData)
		{
			var nomenclatureStocks = aggregatedData.NomenclatureStocks
				.ToDictionary(x => x.NomenclatureId, x => x.Stock);

			//Если по выбранным складам не было движений номенклатуры, то она будет отсутствовать в итоговой выборке.
			//Поэтому просто берем баланс по Id номенклатуры и если ее там нет, вернется дефолтное значение 0
			foreach(var parameter in aggregatedData.NomenclatureParameters)
			{
				nomenclatureStocks.TryGetValue(parameter.NomenclatureId, out var balance);

				if(balance <= 0
					&& parameter.Category != NomenclatureCategory.service
					&& parameter.Category != NomenclatureCategory.master)
				{
					parameter.AvailableForSale = GoodsOnlineAvailability.Show;
				}
			}

			return _saleItemFactory.CreateSelItemPricesDto(aggregatedData.NomenclatureParameters, aggregatedData.NomenclaturePrices);
		}

		private IEnumerable<SaleItemPricesDto> ProcessPromoSets(AggregatedSaleItemPrices aggregatedData)
		{
			var promoSetParameters = aggregatedData.PromoSetParameters
				.ToDictionary(x => x.ErpId);
			var promotionalSetItems = aggregatedData.PromoSetItemPrices
				.ToLookup(x => x.PromotionalSetId);

			var itemsWithZeroBalance =
				promotionalSetItems.SelectMany(keyPairValue =>
					keyPairValue.Where(x => x.Stock <= 0));

			foreach(var item in itemsWithZeroBalance)
			{
				promoSetParameters[item.PromotionalSetId].AvailableForSale = GoodsOnlineAvailability.Show;
			}

			foreach(var promoSetParameter in aggregatedData.PromoSetParameters)
			{
				var items = promotionalSetItems[promoSetParameter.ErpId];

				var promoSetPrices = CalculatePromoSetPrices(items);

				var prices = new[]
				{
					SaleItemPriceDto.CreatePromoSetItem(promoSetParameter.ErpId, promoSetPrices.Sum, promoSetPrices.SumWithoutDiscount),
				};

				promoSetParameter.Prices = prices;
			}

			return aggregatedData.PromoSetParameters;
		}

		private (decimal SumWithoutDiscount, decimal Sum) CalculatePromoSetPrices(IEnumerable<PromotionalSetItemBalanceDto> items)
		{
			var sum = 0m;
			var sumWithoutDiscount = 0m;

			foreach(var item in items)
			{
				var itemSumWithoutDiscount = item.Count * item.NomenclaturePrice;

				var discountMoney = item.IsDiscountMoney
					? item.Discount
					: itemSumWithoutDiscount * item.Discount / 100;

				sum += Math.Round(itemSumWithoutDiscount - discountMoney, 2);
				sumWithoutDiscount += itemSumWithoutDiscount;
			}

			return (sumWithoutDiscount, sum);
		}
	}
}
