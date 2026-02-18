using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Данные запроса по получению доступных типов оплат
	/// </summary>
	public class GetAvailablePaymentMethodsDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
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
		public int CounterpartyErpId { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
	}
}
