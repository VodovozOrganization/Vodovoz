using Vodovoz.Core.Domain.Clients;

namespace CustomerNotifications.Contracts
{
	/// <summary>
	/// Событие интеграции для уведомлений клиентов. Содержит информацию о том, какое уведомление нужно отправить и его данные.
	/// </summary>
	public class CustomerNotificationIntegrationEvent
	{
		/// <summary>
		/// Сообщение для отправки уведомления клиенту о статусе заказа
		/// </summary>
		public CustomerNotificationMessage Payload { get; set; }

		/// <summary>
		/// Внешний источник события
		/// </summary>
		public Source? EventSource { get; set; }

	}
}
