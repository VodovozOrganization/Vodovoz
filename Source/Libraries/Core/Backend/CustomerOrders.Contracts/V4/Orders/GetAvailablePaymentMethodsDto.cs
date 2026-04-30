using System;

namespace CustomerOrders.Contracts.V4.Orders
{
	/// <summary>
	/// Данные запроса по получению доступных типов оплат
	/// </summary>
	public class GetAvailablePaymentMethodsDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Номер заказа в ДВ
		/// </summary>
		public int? OrderId { get; set; }
		/// <summary>
		/// Номер онлайн заказа в ДВ
		/// </summary>
		public int OnlineOrderId { get; set; }
		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
	}
}
