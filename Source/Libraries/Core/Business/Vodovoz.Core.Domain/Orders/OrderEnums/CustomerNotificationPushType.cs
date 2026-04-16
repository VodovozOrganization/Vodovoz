using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Тип пуш уведомления
	/// </summary>
	public enum CustomerNotificationPushType
	{
		[Display(Name = "Отправка без push уведомления")]
		Silent,

		[Display(Name = "Открыть раздел/страницу в МП")]
		Navigate,

		[Display(Name = "Открыть и обновить страницу")]
		NavigateRefresh
	}
}
