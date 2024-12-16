namespace DriverApi.Contracts.V5
{
	/// <summary>
	/// Тип события для уведомлений
	/// </summary>
	public enum PushNotificationDataEventType
	{
		/// <summary>
		/// Изменение состава МЛ
		/// </summary>				
		RouteListContentChanged,

		/// <summary>
		/// Передача заказа от водителя
		/// </summary>
		OrderTransferFrom,

		/// <summary>
		/// Получение заказа водителем
		/// </summary>
		OrderTransferTo
	}
}
