using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Converters;
using CustomerAppsApi.Library.V2.Dto.Goods;
using CustomerAppsApi.Library.V2.Factories;
using CustomerAppsApi.Library.V2.Repositories;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Settings.Common;

namespace CustomerAppsApi.Library.V2.Services
{
	public class SaleItemService : ISaleItemService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGeneralSettings _generalSettings;
		private readonly ISaleItemRepository _saleItemRepository;
		private readonly ISourceConverter _sourceConverter;
		private readonly ISaleItemFactory _saleItemFactory;

		public SaleItemService(
			IUnitOfWorkFactory uowFactory,
			IGeneralSettings generalSettings,
			ISaleItemRepository saleItemRepository,
			ISourceConverter sourceConverter,
			ISaleItemFactory saleItemFactory
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
			_saleItemRepository = saleItemRepository ?? throw new ArgumentNullException(nameof(saleItemRepository));
			_sourceConverter = sourceConverter ?? throw new ArgumentNullException(nameof(sourceConverter));
			_saleItemFactory = saleItemFactory ?? throw new ArgumentNullException(nameof(saleItemFactory));
		}

		/// <inheritdoc/>
		public async Task<SaleItemsPricesAndStockDto> GetSaleItemsPricesAndStocks(Source source)
		{
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var warehouses = _generalSettings.WarehousesForPricesAndStocksIntegration;
			
			using var batchUow = _uowFactory.CreateWithoutRoot();
			using var stocksUow = _uowFactory.CreateWithoutRoot();
			using var promoSetItemsUow = _uowFactory.CreateWithoutRoot();
			var aggregatedDataTask = _saleItemRepository.GetAggregatedSaleItemPricesAsync(batchUow, parameterType);
			//TODO доработать выборку остатков и данных по товарам промонаборов. Они самые тяжелые выполняются от 6 до 10сек
			var stocksTask = _saleItemRepository.GetNomenclaturesForSendInStock(stocksUow, parameterType, warehouses);
			var promoSetItemsPricesTask = _saleItemRepository.GetPromotionalSetsItemsWithBalanceForSend(promoSetItemsUow, parameterType, warehouses);
			
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

			return _saleItemFactory.CreateSaleItemsPricesAndStockDto(allSaleItemPrices);
		}

		/// <inheritdoc/>
		public async Task<SaleItemsDto> GetSaleItems(Source source)
		{
			using var uow = _uowFactory.CreateWithoutRoot();
			var parameterType = _sourceConverter.ConvertToNomenclatureOnlineParameterType(source);
			var data = await _saleItemRepository.GetAggregatedSaleItemsAsync(uow, parameterType);

			var availableWaterIds = data.Nomenclatures
				.Where(x => x.Category == NomenclatureCategory.water)
				.Where(x => x.TareVolume == TareVolume.Vol19L)
				.Select(x => x.ErpId)
				.ToList();

			return _saleItemFactory.CreateSaleItemsDto(data, availableWaterIds);
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
				
				var prices = new[]
				{
					SaleItemPriceDto.CreatePromoSetItem(promoSetParameter.ErpId, CalculatePromoSetPrice(items))
				};
				
				promoSetParameter.Prices = prices;
			}
			
			return aggregatedData.PromoSetParameters;
		}

		private decimal CalculatePromoSetPrice(IEnumerable<PromotionalSetItemBalanceDto> items)
		{
			return (
				from item in items
				let sumWithoutDiscount = item.Count * item.NomenclaturePrice
				let discountMoney = item.IsDiscountMoney
					? item.Discount
					: sumWithoutDiscount * item.Discount / 100
				select Math.Round(sumWithoutDiscount - discountMoney, 2))
				.Sum();
		}
	}
}
