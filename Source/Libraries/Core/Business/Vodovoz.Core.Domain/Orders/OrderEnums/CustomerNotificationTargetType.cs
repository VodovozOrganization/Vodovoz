namespace Vodovoz.Core.Domain.Orders.OrderEnums
{
	/// <summary>
	/// Куда ведёт пуш
	/// </summary>
	public enum CustomerNotificationTargetType
	{
		/// <summary>
		/// Страница заказа
		/// </summary>
		OrderInfo,
		
		/// <summary>
		/// Корзина
		/// </summary>
		Cart,

		/// <summary>
		/// Главная страница МП
		/// </summary>
		Home 
	}
}
