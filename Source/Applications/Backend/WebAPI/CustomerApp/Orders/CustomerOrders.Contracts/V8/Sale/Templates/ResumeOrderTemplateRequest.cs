using System;
using System.Text.Json.Serialization;
using CustomerApp.Contracts.Common;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные запроса на возобновление автозаказа
	/// </summary>
	public class ResumeOrderTemplateRequest
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

		/// <summary>
		/// Ближайшая дата доставки
		/// </summary>
		public DateTime NextDeliveryDate { get; set; }

		[JsonIgnore]
		public DateTime RequestTime { get; } = DateTime.Now;
	}
}
