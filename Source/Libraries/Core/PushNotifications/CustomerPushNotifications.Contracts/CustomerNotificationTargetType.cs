namespace CustomerPushNotifications.Contracts
{
	/// <summary>
	/// Куда ведёт пуш
	/// </summary>
	public enum CustomerNotificationTargetType
	{
		/// Страница заказа
		OrderInfo,
		
		// Корзина
		Cart,

		// Главная страница МП
		Home 
	}
}
