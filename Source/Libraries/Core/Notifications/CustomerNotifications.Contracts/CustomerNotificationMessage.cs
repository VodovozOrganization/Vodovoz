using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace CustomerNotifications.Contracts
{
	/// <summary>
	/// Сообщение для отправки уведомления клиенту о статусе заказа
	/// </summary>
	public class CustomerNotificationMessage
	{
		/// <summary>
		/// Erp id пользователя, которому нужно доставить push, при null- пуш отправляется всем
		/// </summary>
		public int CounterpartyErpId { get; set; }
		
		/// <summary>
		/// Тип пуш уведомления
		/// </summary>
		public CustomerNotificationPushType Type {get; set;}
		
		/// <summary>
		/// Куда ведет пуш
		/// </summary>
		public CustomerNotificationTargetType Target {get; set;}
		
		/// <summary>
		/// Заголовок пуша
		/// </summary>
		public string Title { get; set; }
		
		/// <summary>
		/// Текст пуша
		/// </summary>
		public string Text { get; set; }
		
		/// <summary>
		/// Параметры для навигации
		/// </summary>
		public Dictionary<string, string> Params {get; set;}
	}
}
