namespace CustomerOrdersApi.Library.V6.Dto.Orders
{
	/// <summary>
	/// Статус отслеживания курьера
	/// </summary>
	public enum CourierTrackingStatusTypeDto
	{
		/// <summary>
		/// Курьер отслеживается
		/// </summary>
		Active,
		/// <summary>
		/// Курьер доставил заказ
		/// </summary>
		Complete,
		/// <summary>
		/// Координаты курьера не обновлялись длительное время
		/// </summary>
		Lost
	}
}
