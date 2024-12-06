namespace RoboAtsService.Contracts.Requests
{
	/// <summary>
	/// Тип запроса последнего заказа
	/// </summary>
	public enum LastOrderRequestType
	{
		/// <summary>
		/// Существует
		/// </summary>
		LastOrderExist,
		/// <summary>
		/// Получение идентификатора последнего заказа
		/// </summary>
		GetLastOrderId,
		/// <summary>
		/// Количество воды
		/// </summary>
		WaterQuantity,
		/// <summary>
		/// Количество возвращаемых бутылей
		/// </summary>
		BottlesReturn
	}
}
