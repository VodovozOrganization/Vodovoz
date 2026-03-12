namespace WarehouseApi.Contracts.V1.Responses
{
	/// <summary>
	/// Ответ на запрос получения информации о заказах для самовывоза
	/// </summary>
	public class GetSelfDeliveryOrderResponse
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Клиент
		/// </summary>
		public string Client { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal Sum { get; set; }
	}
}
