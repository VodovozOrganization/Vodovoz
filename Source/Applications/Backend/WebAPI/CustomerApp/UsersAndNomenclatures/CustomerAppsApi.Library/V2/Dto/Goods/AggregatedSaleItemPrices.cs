using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	public class AggregatedSaleItemPrices
	{
		/// <summary>
		/// Онлайн параметры номенклатур
		/// </summary>
		public IEnumerable<NomenclatureOnlineParametersDto> NomenclatureParameters { get; private set; }
		/// <summary>
		/// Цены номенклатур
		/// </summary>
		public IEnumerable<NomenclatureOnlinePriceDto> NomenclaturePrices { get; private set; }
		/// <summary>
		/// Остатки на складах
		/// </summary>
		public IEnumerable<(int NomenclatureId, decimal Stock)> NomenclatureStocks { get; private set; }
		/// <summary>
		/// Онлайн параметры промонаборов
		/// </summary>
		public IEnumerable<SaleItemPricesDto> PromoSetParameters { get; private set; }
		/// <summary>
		/// Цены товаров промоноаборов
		/// </summary>
		public IEnumerable<PromotionalSetItemBalanceDto> PromoSetItemPrices { get; private set; }
		/// <summary>
		/// Данные по пакетам аренды
		/// </summary>
		public IEnumerable<SaleItemPricesDto> RentPackagePrices { get; private set; }
		
		/// <summary>
		/// Добавление данных товаров промонаборов для подсчета цены промонабора
		/// </summary>
		/// <param name="promoSetItemPrices">Данные товаров</param>
		/// <returns></returns>
		public AggregatedSaleItemPrices AddPromoSetItemPrices(IEnumerable<PromotionalSetItemBalanceDto> promoSetItemPrices)
		{
			PromoSetItemPrices = promoSetItemPrices;
			return this;
		}
		
		/// <summary>
		/// Добавление остатков на складах продаваемых номенклатур
		/// </summary>
		/// <param name="nomenclatureStocks">Остатки</param>
		/// <returns></returns>
		public AggregatedSaleItemPrices AddNomenclatureStocks(IEnumerable<(int NomenclatureId, decimal Stock)> nomenclatureStocks)
		{
			NomenclatureStocks = nomenclatureStocks;
			return this;
		}

		public static AggregatedSaleItemPrices Create(
			IEnumerable<NomenclatureOnlineParametersDto> nomenclatureParameters,
			IEnumerable<NomenclatureOnlinePriceDto> nomenclaturePrices,
			IEnumerable<SaleItemPricesDto> promoSetParameters,
			IEnumerable<SaleItemPricesDto> rentPackagePrices
		)
		{
			return new AggregatedSaleItemPrices
			{
				NomenclatureParameters = nomenclatureParameters,
				NomenclaturePrices = nomenclaturePrices,
				PromoSetParameters = promoSetParameters,
				RentPackagePrices = rentPackagePrices
			};
		}

		public static AggregatedSaleItemPrices Create(
			IEnumerable<NomenclatureOnlineParametersDto> nomenclatureParameters,
			IEnumerable<NomenclatureOnlinePriceDto> nomenclaturePrices,
			IEnumerable<SaleItemPricesDto> promoSetParameters,
			IEnumerable<PromotionalSetItemBalanceDto> promoSetItemPrices,
			IEnumerable<SaleItemPricesDto> rentPackagePrices
		)
		{
			return new AggregatedSaleItemPrices
			{
				NomenclatureParameters = nomenclatureParameters,
				NomenclaturePrices = nomenclaturePrices,
				PromoSetParameters = promoSetParameters,
				PromoSetItemPrices = promoSetItemPrices,
				RentPackagePrices = rentPackagePrices
			};
		}
	}
}
