using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Классификация уведомлений клиента
	/// </summary>
	public enum CustomerNotificationClassification
	{
		/// <summary>
		/// Уведомления об акциях
		/// </summary>
		[Display(Name = "Уведомления об акциях")]
		Promotional,

		/// <summary>
		/// Рекламные уведомления
		/// </summary>
		[Display(Name = "Рекламные уведомления")]
		Advertising,

		/// <summary>
		/// Уведомления о доставке
		/// </summary>
		[Display(Name = "Уведомления о доставке")]
		Delivery,

		/// <summary>
		/// Уведомления об оплате
		/// </summary>
		[Display(Name = "Уведомления об оплате")]
		Payment,

		/// <summary>
		/// Системные сообщения
		/// </summary>
		[Display(Name = "Системные сообщения")]
		System
	}
}
