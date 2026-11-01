using System;
using CustomerApp.Contracts.Common;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные запроса на приостановку автозаказа
	/// </summary>
	public class PauseOrderTemplateRequest
	{
		/// <summary>
		/// ИПЗ
		/// </summary>
		public ExternalSource Source { get; set; }

		/// <summary>
		/// Идентификатор клиента
		/// </summary>
		public int? ErpCounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор пользователя
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор шаблона автозаказа
		/// </summary>
		public int OrderTemplateId { get; set; }
	}
}
