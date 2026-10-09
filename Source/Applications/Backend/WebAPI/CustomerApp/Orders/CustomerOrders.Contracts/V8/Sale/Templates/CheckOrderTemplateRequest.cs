using System;
using CustomerApp.Contracts.Common;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные запроса проверки параметров автозаказа
	/// </summary>
	public class CheckOrderTemplateRequest
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
		/// Идентификатор онлайн заказа
		/// </summary>
		public int OnlineOrderId { get; set; }
		
		/// <summary>
		/// Выбранные параметры доставки
		/// </summary>
		public TemplateDeliveryScheduleDto Schedule { get; set; }
	}
}
