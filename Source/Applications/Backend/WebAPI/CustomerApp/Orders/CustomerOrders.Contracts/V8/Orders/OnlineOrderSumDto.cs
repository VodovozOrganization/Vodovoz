namespace CustomerOrders.Contracts.V8.Orders
{
	public sealed class OnlineOrderSumDto
	{
		/// <summary>
		/// Сумма товаров, без учета доставки и скидок
		/// </summary>
		public decimal RawSum { get; set; }
		
		/// <summary>
		/// Общая скидка
		/// </summary>
		public decimal Discount { get; set; }
		
		/// <summary>
		/// Доставка
		/// </summary>
		public decimal Delivery { get; set; }
		
		/// <summary>
		/// Итоговая стоимость
		/// </summary>
		public decimal Total { get; set; }
	}
}
