using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto.Goods
{
	/// <summary>
	/// Данные по выгружаемым позициям в ИПЗ
	/// </summary>
	public class AggregatedSaleItems
	{
		/// <summary>
		/// Список данных по номенклатурам
		/// </summary>
		public IEnumerable<OnlineNomenclatureDto> Nomenclatures { get; private set; }
		/// <summary>
		/// Список данных по промонаборам
		/// </summary>
		public IEnumerable<PromotionalSetDto> PromoSets { get; private set; }
		/// <summary>
		/// Список данных по пакетам аренды
		/// </summary>
		public IEnumerable<FreeRentPackageDto> RentPackages { get; private set; }

		public static AggregatedSaleItems Create(
			IEnumerable<OnlineNomenclatureDto> nomenclatures,
			IEnumerable<PromotionalSetDto> promoSets,
			IEnumerable<FreeRentPackageDto> rentPackages)
		{
			return new AggregatedSaleItems
			{
				Nomenclatures = nomenclatures,
				PromoSets = promoSets,
				RentPackages = rentPackages
			};
		}
	}
}
