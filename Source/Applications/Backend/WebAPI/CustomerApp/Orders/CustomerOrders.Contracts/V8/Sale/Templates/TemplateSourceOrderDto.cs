namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные по первому заказу, к которому прикреплен автозаказ
	/// </summary>
	public class TemplateSourceOrderDto
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// Номер онлайн заказа
		/// </summary>
		public int? OnlineOrderId { get; set; }

		public static TemplateSourceOrderDto Create(int? onlineOrderId, int? orderId = null)
		{
			return new TemplateSourceOrderDto
			{
				OrderId = orderId,
				OnlineOrderId = onlineOrderId
			};
		}
	}
}
