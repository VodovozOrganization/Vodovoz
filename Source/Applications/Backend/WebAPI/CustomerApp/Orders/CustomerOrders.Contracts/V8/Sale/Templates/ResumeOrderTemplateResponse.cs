using System;
using System.Collections.Generic;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные ответа по возобновлению автозаказа
	/// </summary>
	public class ResumeOrderTemplateResponse
	{
		/// <summary>
		/// Статус шаблона
		/// </summary>
		public TemplateStatusDto Status { get; set; }

		/// <summary>
		/// Идентификатор шаблона автозаказа
		/// </summary>
		public int OrderTemplateId { get; set; }

		/// <summary>
		/// Ближайшая дата доставки
		/// </summary>
		public DateTime NextDeliveryDate { get; set; }

		/// <summary>
		/// Ближайшие доставки
		/// </summary>
		public IEnumerable<TemplateNextDeliveriesDto> NextDeliveries { get; set; }

		public static ResumeOrderTemplateResponse Create(
			int orderTemplateId,
			TemplateStatusDto status,
			DateTime nextDeliveryDate,
			IEnumerable<TemplateNextDeliveriesDto> nextDeliveries) =>
			new ResumeOrderTemplateResponse
			{
				OrderTemplateId = orderTemplateId,
				Status = status,
				NextDeliveryDate = nextDeliveryDate,
				NextDeliveries = nextDeliveries
			};
	}
}
