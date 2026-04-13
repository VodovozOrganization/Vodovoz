using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Классификация уведомлений клиента
	/// </summary>
	public enum CustomerNotificationClassification
	{
		[Display(Name = "Уведомления об акциях")]
		Promotional,

		[Display(Name = "Рекламные уведомления")]
		Advertising,

		[Display(Name = "Уведомления о доставке")]
		Delivery,

		[Display(Name = "Уведомления об оплате")]
		Payment,

		[Display(Name = "Системные сообщения")]
		System
	}
}
