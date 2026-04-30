namespace CustomerOrders.Contracts.Default.Orders
{
	/// <summary>
	/// Данные для получения детализированной информации о заказе
	/// </summary>
	public class GetDetailedOrderInfoDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }
		/// <summary>
		/// Контрольная сумма запроса
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Номер заказа в ДВ
		/// </summary>
		public int? OrderId { get; set; }
		/// <summary>
		/// Номер онлайн заказа в ДВ
		/// </summary>
		public int? OnlineOrderId { get; set; }
	}
}
