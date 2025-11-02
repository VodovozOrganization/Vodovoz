namespace RoboatsService.Requests
{
	/// <summary>
	/// Тип запроса
	/// </summary>
	public enum OrderRequestType
	{
		/// <summary>
		/// Неизвестный
		/// </summary>
		Unknown,
		/// <summary>
		/// Создание заказа
		/// </summary>
		CreateOrder,
		/// <summary>
		/// Проверка цены
		/// </summary>
		PriceCheck
	}
}
