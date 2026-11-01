using System.Collections.Generic;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Ближайшие даты доставки автозаказа
	/// </summary>
	public class GetOrderTemplateResumeDatesResponse
	{
		/// <summary>
		/// Ближайшие доставки автозаказа
		/// </summary>
		public IEnumerable<TemplateNextDeliveriesDto> DeliveryDates { get; set; }

		public static GetOrderTemplateResumeDatesResponse Create(IEnumerable<TemplateNextDeliveriesDto> deliveryDates) =>
			new GetOrderTemplateResumeDatesResponse
			{
				DeliveryDates = deliveryDates
			};
	}
}
