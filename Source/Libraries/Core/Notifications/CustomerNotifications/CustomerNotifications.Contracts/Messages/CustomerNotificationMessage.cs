using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Contracts.Messages
{
	/// <summary>
	/// Сообщение для отправки push-уведомления клиенту о статусе заказа
	/// </summary>
	public class CustomerNotificationMessage
	{
		/// <summary>
		/// Код онлайн-заказа в ERP
		/// </summary>
		public int OnlineOrderId { get; set; }

		/// <summary>
		/// Тип события для уведомления
		/// </summary>
		public CustomerNotificationEventType CustomerNotificationEventType { get; set; }
	}
}
