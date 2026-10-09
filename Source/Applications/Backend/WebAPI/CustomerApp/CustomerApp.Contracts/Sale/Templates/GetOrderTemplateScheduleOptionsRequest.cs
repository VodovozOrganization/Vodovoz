using System;
using CustomerApp.Contracts.Common;

namespace CustomerApp.Contracts.Sale.Templates
{
	public class GetOrderTemplateScheduleOptionsRequest
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Id онлайн заказа в ДВ
		/// </summary>
		public int OnlineOrderId { get; set; }
	}
}
