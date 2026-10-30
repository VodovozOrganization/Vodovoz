using System;
using CustomerApp.Contracts.Common;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	public class GetOrderTemplateInfoDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }

		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор пользователя
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор автозаказа
		/// </summary>
		public int OrderTemplateId { get; set; }
	}
}
