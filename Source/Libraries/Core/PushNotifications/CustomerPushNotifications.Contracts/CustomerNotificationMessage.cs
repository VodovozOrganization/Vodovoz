using System.Collections.Generic;

namespace CustomerPushNotifications.Contracts
{
	/// <summary>
	/// Сообщение для отправки push-уведомления клиенту о статусе заказа
	/// </summary>
	public class CustomerNotificationMessage
	{
		// Erp id пользователя, которому нужно доставить push, при null- пуш отправляется всем
		public int? CounterpartyErpId { get; set; }
		
		// Тип пуш уведомления
		public CustomerNotificationPushType Type {get; set;}
		
		// Куда ведет пуш
		public CustomerNotificationTargetType Target {get; set;}
		
		// Заголовок пуша
		public string Title { get; set; }
		
		// Текст пуша
		public string Text { get; set; }
		
		// Параметры для навигации
		public Dictionary<string, string> Params {get; set;}
	}
}
