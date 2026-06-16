using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Тип пуш уведомления
	/// </summary>
	public enum CustomerNotificationPushType
	{
		/// <summary>
		/// Отправка без push уведомления
		/// </summary>
		[Display(Name = "Отправка без push уведомления")]
		Silent,

		/// <summary>
		/// Открыть раздел/страницу в МП
		/// </summary>
		[Display(Name = "Открыть раздел/страницу в МП")]
		Navigate,

		/// <summary>
		/// Открыть и обновить страницу
		/// </summary>
		[Display(Name = "Открыть и обновить страницу")]
		NavigateRefresh
	}
}
