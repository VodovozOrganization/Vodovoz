using CustomerOrders.Contracts.V8.Sale.Templates;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Dto.Templates
{
	/// <summary>
	/// Данные для формирования карточки шаблона автозаказа(для списка шаблонов)
	/// </summary>
	public class OrderTemplateCardRawData
	{
		/// <summary>
		/// Неполные данные об автозаказе (шаблоне) <see cref="OrderTemplateInfoDto"/>
		/// </summary>
		public OrderTemplateCardDto TemplateCard { get; set; }

		/// <summary>
		/// Данные по доставке из шаблона <see cref="TemplateDeliveryScheduleData"/>
		/// </summary>
		public TemplateDeliveryScheduleData Schedule { get; set; }

		/// <summary>
		/// Данные по интервалу доставки <see cref="DeliveryScheduleData"/>
		/// </summary>
		public DeliveryScheduleData DeliveryInterval { get; set; }

		public static OrderTemplateCardRawData Create(
			OrderTemplateCardDto templateCard,
			TemplateDeliveryScheduleData schedule,
			DeliveryScheduleData deliveryInterval
		)
		{
			return new OrderTemplateCardRawData
			{
				TemplateCard = templateCard,
				Schedule = schedule,
				DeliveryInterval = deliveryInterval
			};
		}
	}
}
