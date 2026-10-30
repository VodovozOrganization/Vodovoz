using System.Collections.Generic;
using CustomerOrders.Contracts.V8.Sale.Templates;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Dto.Templates
{
	/// <summary>
	/// Данные для формирования детализации шаблона автозаказа
	/// </summary>
	public class OrderTemplateInfoRawData
	{
		/// <summary>
		/// Неполные данные об автозаказе (шаблоне) <see cref="OrderTemplateInfoDto"/>
		/// </summary>
		public OrderTemplateInfoDto TemplateInfo { get; set; }

		/// <summary>
		/// Данные по доставке из шаблона <see cref="TemplateDeliveryScheduleData"/>
		/// </summary>
		public TemplateDeliveryScheduleData Schedule { get; set; }

		/// <summary>
		/// Данные по интервалу доставки <see cref="DeliveryScheduleData"/>
		/// </summary>
		public DeliveryScheduleData DeliveryInterval { get; set; }

		/// <summary>
		/// Позиции на продажу из шаблона <see cref="OnlineOrderTemplateSaleItem"/>
		/// </summary>
		public IEnumerable<OnlineOrderTemplateSaleItem> SaleItems { get; set; }

		/// <summary>
		/// Промонаборы из шаблона <see cref="OnlineOrderTemplatePromoSet"/>
		/// </summary>
		public IEnumerable<OnlineOrderTemplatePromoSet> PromoSets { get; set; }

		/// <summary>
		/// Пакеты аренды из шаблона <see cref="OnlineOrderTemplateFreeRentPackage"/>
		/// </summary>
		public IEnumerable<OnlineOrderTemplateFreeRentPackage> RentPackages { get; set; }

		/// <summary>
		/// Фиксированные цены точки доставки
		/// </summary>
		public IEnumerable<NomenclatureFixedPrice> DeliveryPointFixedPrices { get; set; }

		/// <summary>
		/// Фиксированные цены клиента
		/// </summary>
		public IEnumerable<NomenclatureFixedPrice> CounterpartyFixedPrices { get; set; }

		public static OrderTemplateInfoRawData Create(
			OrderTemplateInfoDto templateInfo,
			TemplateDeliveryScheduleData schedule,
			DeliveryScheduleData deliveryInterval
		)
		{
			return new OrderTemplateInfoRawData
			{
				TemplateInfo = templateInfo,
				Schedule = schedule,
				DeliveryInterval = deliveryInterval
			};
		}
	}
}
