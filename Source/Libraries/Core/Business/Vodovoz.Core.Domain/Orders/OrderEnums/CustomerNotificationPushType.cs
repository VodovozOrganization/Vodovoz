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
		Silent,
		
		/// <summary>
		/// Открыть раздел/страницу в МП
		/// </summary>
		Navigate,
		
		/// <summary>
		/// Открыть и обновить страницу
		/// </summary>
		NavigateRefresh
	}
}
