using System;
using CustomerApp.Contracts.Common;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class CreateOrderTemplateRequest
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }

		/// <summary>
		/// ID контрагента в ERP
		/// </summary>
		public int ErpCounterpartyId { get; set; }

		/// <summary>
		/// Внешний ID контрагента
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }

		/// <summary>
		/// ID онлайн-заказа
		/// </summary>
		public int OnlineOrderId { get; set; }

		/// <summary>
		/// График доставки
		/// </summary>
		public TemplateDeliveryScheduleDto Schedule { get; set; }
	}
}
