using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Dto.Templates
{
	/// <summary>
	/// Данные для формирования карточки шаблона автозаказа(для списка шаблонов)
	/// </summary>
	public class OrderTemplateCardsRawData
	{
		public IEnumerable<OrderTemplateCardRawData> TemplateCards { get; set; }

		/// <summary>
		/// Позиции на продажу из шаблона <see cref="OnlineOrderTemplateSaleItem"/>
		/// </summary>
		public ILookup<int, OnlineOrderTemplateSaleItem> SaleItems { get; set; }

		/// <summary>
		/// Промонаборы из шаблона <see cref="OnlineOrderTemplatePromoSet"/>
		/// </summary>
		public ILookup<int, OnlineOrderTemplatePromoSet> PromoSets { get; set; }

		/// <summary>
		/// Пакеты аренды из шаблона <see cref="OnlineOrderTemplateFreeRentPackage"/>
		/// </summary>
		public ILookup<int, OnlineOrderTemplateFreeRentPackage> RentPackages { get; set; }

		/// <summary>
		/// Фиксированные цены точки доставки
		/// </summary>
		public ILookup<int, NomenclatureFixedPrice> DeliveryPointFixedPrices { get; set; }

		/// <summary>
		/// Фиксированные цены клиента
		/// </summary>
		public ILookup<int, NomenclatureFixedPrice> CounterpartyFixedPrices { get; set; }

		public static OrderTemplateCardsRawData Create(
			IEnumerable<OrderTemplateCardRawData> templateCards,
			ILookup<int, OnlineOrderTemplateSaleItem> saleItems,
			ILookup<int, OnlineOrderTemplatePromoSet> promoSets,
			ILookup<int, OnlineOrderTemplateFreeRentPackage> rentPackages,
			ILookup<int, NomenclatureFixedPrice> deliveryPointFixedPrices,
			ILookup<int, NomenclatureFixedPrice> counterpartyFixedPrices
		)
		{
			return new OrderTemplateCardsRawData
			{
				TemplateCards = templateCards,
				SaleItems = saleItems,
				PromoSets = promoSets,
				RentPackages = rentPackages,
				DeliveryPointFixedPrices = deliveryPointFixedPrices,
				CounterpartyFixedPrices = counterpartyFixedPrices
			};
		}
	}
}
